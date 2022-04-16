using System;
using System.IO;
using Xunit;
using PathLib;
using Path = System.IO.Path;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

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

            var st_dev = Regex.Match(output, @"Device:\s\w+/(\d+)d").Groups[1].Value;

            var info = new PosixPath(path).Stat();

            Assert.Equal(st_dev, info.Device.ToString());
        }
    }

    [Fact]
    public void Native()
    {
        const string contents = "Hello World";
        var fname = Guid.NewGuid().ToString();
        var path = Path.Combine(_fixture.TempFolder, fname);
        File.WriteAllText(path, contents);

        
        var err = PathLib.Posix.Native.stat64(path, out var info);
        Assert.Equal(11L, info.st_size);
    }

    [Fact]
    public void PPath()
    {
        const string contents = "Hello World";
        var fname = Guid.NewGuid().ToString();
        var path = Path.Combine(_fixture.TempFolder, fname);
        File.WriteAllText(path, contents);
        var ppath = new PurePosixPath(path);

        var err = PathLib.Posix.Native.stat64(ppath.ToString(), out var info);
        Assert.Equal(11L, info.st_size);
    }

    [Fact]
    public void ZFull()
    {
        const string contents = "Hello World";
        var fname = Guid.NewGuid().ToString();
        var path = Path.Combine(_fixture.TempFolder, fname);
        File.WriteAllText(path, contents);
        var ppath = new PosixPath(path);

        var info = ppath.Stat();
        Assert.NotNull(info);
        Assert.Equal(11L, info.Size);
    }
}