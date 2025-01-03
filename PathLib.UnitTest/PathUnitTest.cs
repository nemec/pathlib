using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace PathLib.UnitTest
{

    public class PathTestsFixture : IDisposable
    {
        public string TempFolder { get; }
        
        public bool IsWindows { get; set;  }

        public PathTestsFixture()
        {
            do
            {
                TempFolder = Path.Combine(Path.GetTempPath(), "pathlib_" + Guid.NewGuid().ToString());
            } while (Directory.Exists(TempFolder));
            Directory.CreateDirectory(TempFolder);
            
            IsWindows = true;
            var p = Environment.OSVersion.Platform;
            // http://mono.wikia.com/wiki/Detecting_the_execution_platform
            // 128 required for early versions of Mono
            if (p == PlatformID.Unix || p == PlatformID.MacOSX || (int)p == 128)
            {
                IsWindows = false;
            }
        }

        public void Dispose()
        {
            Directory.Delete(TempFolder, true);
        }
    }
    
    public class PathUnitTest : IClassFixture<PathTestsFixture>
    {
        private readonly PathTestsFixture _fixture;
        
        public PathUnitTest(PathTestsFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        public void Create_WithTypeConverter_CreatesPathForPlatform()
        {

            const string path = @"C:\users\tmp";
            var converter = TypeDescriptor.GetConverter(typeof (IPath));
            var expected = _fixture.IsWindows
                ? typeof (WindowsPath)
                : typeof (PosixPath);

            var actual = converter.ConvertFromInvariantString(path);

            Assert.IsType(expected, actual);
        }

        /*
         * 
            Directory.CreateDirectory(Path.Combine(rootPath, "real1"));
            Directory.CreateDirectory(Path.Combine(rootPath, "real2/real3"));
            Directory.CreateDirectory(Path.Combine(rootPath, "real2/real4"));
            File.Create(Path.Combine(rootPath, "real2/real3/file.txt"));
            File.Create(Path.Combine(rootPath, "real2/real4/anotherfile.txt"));
            File.Create(Path.Combine(rootPath, "real2/absfile.txt"));
            Directory.CreateSymbolicLink(
                Path.Combine(rootPath, "real1/sym1"),
                "../../real2");
            Directory.CreateSymbolicLink(
                Path.Combine(rootPath, "real2/sym3"),
                "../real3");
            Directory.CreateSymbolicLink(
                Path.Combine(rootPath, "sym3"),
                Path.Combine(rootPath, "real2/real3/file.txt"));
         */
        
        [Fact]
        public void Resolve_WithNoSymlinks_KeepsSamePath()
        {
            var rootFilename = Guid.NewGuid().ToString();
            var rootPath = Path.Combine(_fixture.TempFolder, rootFilename);
            File.Create(rootPath).Dispose();
            IPath expected = _fixture.IsWindows
                ? new WindowsPath(rootPath)
                : new PosixPath(rootPath);

            var actual = expected.Resolve();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ListDir_with_pattern()
        {
            // Arrange
            var tmpDir = Paths.Create(_fixture.TempFolder);
            Assert.True(tmpDir.IsDir());

            var file1 = tmpDir / "file1";
            Assert.False(file1.IsFile());
            file1.Open(FileMode.CreateNew).Dispose();
            Assert.True(file1.IsFile());

            var dir1 = tmpDir / "dir1";
            Assert.False(dir1.IsDir());
            dir1.Mkdir();
            Assert.True(dir1.IsDir());

            // Assertions
            tmpDir.ListDir("non-existent").Should().BeEmpty();

            tmpDir.ListDir("file*").Select(f => f.FileInfo.FullName).Should()
                .BeEquivalentTo(file1.FileInfo.FullName);
        }

        // sym1 => ../../real2
        // sym2 => ../real3
        // sym3 => /abs/path/real2/absfile.txt
        // /real1/sym1/real3/file.txt
        // /real2/real3/file.txt
        // /real2/sym3
        // /real2/real4/anotherfile.txt
        // /real2/absfile.txt
        // /sym3
    }
}