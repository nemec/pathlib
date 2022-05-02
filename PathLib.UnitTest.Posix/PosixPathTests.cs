using System;
using System.IO;
using Xunit;
using PathLib;
using Path = System.IO.Path;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Xunit.Sdk;

namespace PathLib.UnitTest.Posix;

public class PosixPathTestsFixture : IDisposable
{
    public string TempFolder { get; set; }

    public PosixPathTestsFixture()
    {
        do
        {
            TempFolder = Path.Combine(Path.GetTempPath(), "pathlib_" + Guid.NewGuid().ToString());
        } while (Directory.Exists(TempFolder));
        Directory.CreateDirectory(TempFolder);
    }

    public void Dispose()
    {
        Directory.Delete(TempFolder, true);
    }
}

public class PosixPathTests : IClassFixture<PosixPathTestsFixture>
{
    private readonly PosixPathTestsFixture _fixture;

    public PosixPathTests(PosixPathTestsFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Stat_With_MissingFile_GivesError()
    {
        var path = new PosixPath(Path.Combine(_fixture.TempFolder, "does_not_exist"));
        Assert.False(path.Exists());

        Assert.Throws<FileNotFoundException>(() => path.Stat());
    }

    [Fact]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public async Task Stat_WithFile_ReturnsStatInfo()
    {
        const string contents = "Hello World";
        var fname = Guid.NewGuid().ToString();
        var path = Path.Combine(_fixture.TempFolder, fname);
        await File.WriteAllTextAsync(path, contents);

        using(var process = new Process())
        {
            process.StartInfo.FileName = "stat";
            process.StartInfo.Arguments = path;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            Assert.Equal(0, process.ExitCode);

            var st_dev = Regex.Match(output, @"Device:\s\w+/(\d+)d").Groups[1].Value;
            var st_ino = Regex.Match(output, @"Inode:\s(\d+)").Groups[1].Value;
            var st_mode = Regex.Match(output, @"Access:\s\((\d+)").Groups[1].Value;
            var st_nlink = Regex.Match(output, @"Links:\s(\d+)").Groups[1].Value;
            var st_uid = Regex.Match(output, @"Uid:\s\(\s(\d+)").Groups[1].Value;
            var st_gid = Regex.Match(output, @"Gid:\s\(\s(\d+)").Groups[1].Value;
            var st_size = Regex.Match(output, @"Size:\s(\d+)").Groups[1].Value;
            var st_atim_match = Regex.Match(output, @"Access:\s([\w: -]+\.\d{7})\d{2} (-\d{2})");
            var st_atim = DateTime.ParseExact(st_atim_match.Groups[1].Value + " " + st_atim_match.Groups[2].Value,
                "yyyy-MM-dd HH:mm:ss.FFFFFFF zz", CultureInfo.InvariantCulture).ToUniversalTime();
            var st_mtim_match = Regex.Match(output, @"Modify:\s([\w: -]+\.\d{7})\d{2} (-\d{2})");
            var st_mtim = DateTime.ParseExact(st_mtim_match.Groups[1].Value + " " + st_mtim_match.Groups[2].Value,
                "yyyy-MM-dd HH:mm:ss.FFFFFFF zz", CultureInfo.InvariantCulture).ToUniversalTime();
            var st_ctim_match = Regex.Match(output, @"Birth:\s([\w: -]+\.\d{7})\d{2} (-\d{2})");
            var st_ctim = DateTime.ParseExact(st_ctim_match.Groups[1].Value + " " + st_ctim_match.Groups[2].Value,
                "yyyy-MM-dd HH:mm:ss.FFFFFFF zz", CultureInfo.InvariantCulture).ToUniversalTime();

            var info = new PosixPath(path).Stat();

            Assert.Equal(st_dev, info.Device.ToString());
            Assert.Equal(st_ino, info.Inode.ToString());
            Assert.Equal(st_mode, info.Mode);
            Assert.Equal(st_nlink, info.NumLinks.ToString());
            Assert.Equal(st_uid, info.Uid.ToString());
            Assert.Equal(st_gid, info.Gid.ToString());
            Assert.Equal(st_size, info.Size.ToString());
            Assert.Equal(st_atim, info.ATime);
            Assert.Equal(st_mtim, info.MTime);
            Assert.Equal(st_ctim, info.CTime);
        }
    }

    [Fact]
    public void ExpandUser_WithHomeDirSet_ReplacesPath()
    {
        var root = Environment.GetEnvironmentVariable("HOME");
        var expected = new PosixPath(root, "tmp");

        var actual = new PosixPath("~/tmp").ExpandUser();
        
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ExpandUser_WithNoHomeDirSet_ReplacesPath()
    {
        var root = Environment.GetEnvironmentVariable("HOME");
        var expected = new PosixPath(root, "tmp");
        Environment.SetEnvironmentVariable("HOME", null);

        var actual = new PosixPath("~/tmp").ExpandUser();
        
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsSymlink_WithFile_ReturnsFalse()
    {
        var path = new PosixPath("/dev/mem");
        Assert.False(path.IsSymlink());
    }

    [Fact]
    public void IsSymlink_WithDirectory_ReturnsFalse()
    {
        var path = new PosixPath("/dev/usb");
        Assert.False(path.IsSymlink());
    }

    [Fact]
    public void IsSymlink_WithMissingFile_ReturnsFalse()
    {
        var path = new PosixPath(Path.Combine(_fixture.TempFolder, "does-not-exist"));
        Assert.False(path.IsSymlink());
    }

    [Fact]
    public void IsSymlink_WithSymlink_ReturnsTrue()
    {
        var path = new PosixPath("/dev/stdout");
        Assert.True(path.IsSymlink());
    }

    [Fact]
    public async Task FileType_WithRegularFile_ReturnsRegularFile()
    {
        const string contents = "Hello World";
        var fname = Guid.NewGuid().ToString();
        var path = Path.Combine(_fixture.TempFolder, fname);
        await File.WriteAllTextAsync(path, contents);
        var fileType = new PosixPath(path).GetFileType();
        Assert.Equal(PathLib.Posix.FileType.RegularFile, fileType);
    }

    [Fact]
    public void FileType_WithDirectory_ReturnsDirectory()
    {
        var fileType = new PosixPath(_fixture.TempFolder).GetFileType();
        Assert.Equal(PathLib.Posix.FileType.Directory, fileType);
    }

    [Fact]
    public void FileType_WithCharacterDevice_ReturnsCharacterDevice()
    {
        var fileType = new PosixPath("/dev/null").GetFileType();
        Assert.Equal(PathLib.Posix.FileType.CharacterDevice, fileType);
    }

    [Fact]
    public void FileType_WithBlockDevice_ReturnsBlockDevice()
    {
        var fileType = new PosixPath("/dev/sda").GetFileType();
        Assert.Equal(PathLib.Posix.FileType.BlockDevice, fileType);
    }

    [Fact]
    public void FileType_WithSocket_ReturnsSocket()
    {
        var fname = Guid.NewGuid().ToString();
        var path = Path.Combine(_fixture.TempFolder, fname);
        var pipeServer = new NamedPipeServerStream(path, PipeDirection.InOut);
        
        var fileType = new PosixPath(path).GetFileType();
        Assert.Equal(PathLib.Posix.FileType.Socket, fileType);
    }
    

    [Fact]
    public void FileType_WithPipe_ReturnsFifo()
    {
        var fname = Guid.NewGuid().ToString();
        var path = Path.Combine(_fixture.TempFolder, fname);
        var err = mkfifo(path, 0x1ED); // 0o755
        if (err != 0)
        {
            var actualError = Marshal.GetLastWin32Error();
            throw new ApplicationException("Error: " + actualError);
        }
        
        var fileType = new PosixPath(path).GetFileType();
        Assert.Equal(PathLib.Posix.FileType.Fifo, fileType);
    }

    [DllImport("libc", SetLastError = true, CharSet = CharSet.Auto, CallingConvention=CallingConvention.Cdecl)]
    private static extern int mkfifo(string path, uint mode);

    [Fact]
    public void FileType_WithFileNotExist_ReturnsFileNotExist()
    {
        var fname = Guid.NewGuid().ToString();
        var path = Path.Combine(_fixture.TempFolder, fname);
        
        var fileType = new PosixPath(path).GetFileType();
        Assert.Equal(PathLib.Posix.FileType.DoesNotExist, fileType);
    }

    [Fact]
    public void SetCurrentDirectory_WithDirectory_SetsEnvironmentVariable()
    {
        const string newCwd = @"/";
        var path = new PosixPath(newCwd);
        using (path.SetCurrentDirectory())
        {
            Assert.Equal(newCwd, Environment.CurrentDirectory);
        }
    }

    [Fact]
    public void SetCurrentDirectory_UponDispose_RestoresEnvironmentVariable()
    {
        var oldCwd = Environment.CurrentDirectory;
        var path = new PosixPath(@"/");
        var tmp = path.SetCurrentDirectory();
            
        tmp.Dispose();

        Assert.Equal(oldCwd, Environment.CurrentDirectory);
    }

    [Fact]
    public void JoinIPath_WithAnotherPath_ReturnsWindowsPath()
    {
        IPath path = new PosixPath(@"/tmp");
        IPath other = new PosixPath(@"/tmp");

        var final = path.Join(other);

        Assert.True(final is PosixPath);
    }

    [Fact]
    public void JoinIPath_WithAnotherPathByDiv_ReturnsWindowsPath()
    {
        IPath path = new PosixPath(@"/tmp");
        IPath other = new PosixPath(@"/tmp");

        var final = path / other;

        Assert.True(final is PosixPath);
    }

    [Fact]
    public void JoinIPath_WithStringByDiv_ReturnsWindowsPath()
    {
        IPath path = new PosixPath(@"/tmp");
        var other = @"/tmp";

        var final = path / other;

        Assert.True(final is PosixPath);
    }

    [Fact]
    public void JoinPosixPath_WithStringByDiv_ReturnsPosixPath()
    {
        var path = new PosixPath(@"/tmp");
        var other = @"/";

        var final = path / other;

        Assert.True(final is PosixPath);
    }
}