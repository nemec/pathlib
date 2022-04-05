using System;
using System.IO;
using Xunit;
using PathLib;
using Path = System.IO.Path;

namespace PathLib.UnitTest.Windows
{
    public class WindowsPathTestsFixture : IDisposable
    {
        public string TempFolder { get; set; }

        public WindowsPathTestsFixture()
        {
            do
            {
                TempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            } while (Directory.Exists(TempFolder));
            Directory.CreateDirectory(TempFolder);
        }

        public void Dispose()
        {
            Directory.Delete(TempFolder, true);
        }
    }

    public class WindowsPathTests : IClassFixture<WindowsPathTestsFixture>
    {
        private readonly WindowsPathTestsFixture _fixture;

        public WindowsPathTests(WindowsPathTestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void IsJunction_WithJunction_ReturnsTrue()
        {
            var ret = TestUtils.CreateJunctionAndTarget(_fixture.TempFolder);
            var junction = ret.Item2;
            
            var path = new WindowsPath(junction);
            
            Assert.True(path.IsJunction());
        }

        [Fact]
        public void SetCurrentDirectory_WithDirectory_SetsEnvironmentVariable()
        {
            const string newCwd = @"C:\";
            var path = new WindowsPath(newCwd);
            using (path.SetCurrentDirectory())
            {
                Assert.Equal(newCwd, Environment.CurrentDirectory);
            }
        }

        [Fact]
        public void SetCurrentDirectory_UponDispose_RestoresEnvironmentVariable()
        {
            var oldCwd = Environment.CurrentDirectory;
            var path = new WindowsPath(@"C:\");
            var tmp = path.SetCurrentDirectory();
            
            tmp.Dispose();

            Assert.Equal(oldCwd, Environment.CurrentDirectory);
        }

        [Fact]
        public void ExpandUser_WithHomeDir_ExpandsDir()
        {
            var path = new WindowsPath("~/tmp");
            var expected = new WindowsPath(
                Environment.GetEnvironmentVariable("USERPROFILE"), "tmp");

            var actual = path.ExpandUser();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ExpandUser_WithCustomHomeDirString_ExpandsDir()
        {
            var homeDir = new WindowsPath(@"C:\users\test");
            var path = new WindowsPath("~/tmp");
            var expected = homeDir.Join("tmp");

            var actual = path.ExpandUser(homeDir);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void JoinIPath_WithAnotherPath_ReturnsWindowsPath()
        {
            IPath path = new WindowsPath(@"C:\tmp");
            IPath other = new WindowsPath(@"C:\tmp");

            var final = path.Join(other);

            Assert.True(final is WindowsPath);
        }

        [Fact]
        public void JoinIPath_WithAnotherPathByDiv_ReturnsWindowsPath()
        {
            IPath path = new WindowsPath(@"C:\tmp");
            IPath other = new WindowsPath(@"C:\tmp");

            var final = path / other;

            Assert.True(final is WindowsPath);
        }

        [Fact]
        public void JoinIPath_WithStringByDiv_ReturnsWindowsPath()
        {
            IPath path = new WindowsPath(@"C:\tmp");
            var other = @"C:\tmp";

            var final = path / other;

            Assert.True(final is WindowsPath);
        }

        [Fact]
        public void JoinWindowsPath_WithStringByDiv_ReturnsWindowsPath()
        {
            var path = new WindowsPath(@"C:\tmp");
            var other = @"C:\tmp";

            var final = path / other;

            Assert.True(final is WindowsPath);
        }

        // TODO FileInfo with nonexistant file
        // TODO FileInfo with directory
        // TODO FileInfo with file

        // TODO DirectoryInfo with nonexistant directory
        // TODO DirectoryInfo with directory
        // TODO DirectoryInfo with file

        // TODO delete file
        // TODO delete directory
        // TODO delete recursive

        
        // TODO resolve path with .. in it (symlinks)

        // TODO listdir with selector
        // TODO listdir with selector case insensitive
    }
}
