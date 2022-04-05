using System;
using Xunit;

namespace PathLib.UnitTest
{
    public class PurePathFactoryUnitTest
    {
        private static readonly bool IsWindows;

        static PurePathFactoryUnitTest()
        {
            var p = Environment.OSVersion.Platform;
            // http://mono.wikia.com/wiki/Detecting_the_execution_platform
            // 128 required for early versions of Mono
            if (p == PlatformID.Unix || p == PlatformID.MacOSX || (int)p == 128)
            {
                IsWindows = false;
            }
            IsWindows = true;
        }

        [Fact]
        public void Create_FromOnePath_CreatesPath()
        {
            const string part1 = "C:\\tmp";
            var factory = new PurePathFactory();
            var expected = IsWindows
                ? (IPurePath)new PureWindowsPath(part1)
                : new PurePosixPath(part1);

            var actual = factory.Create(part1);

            Assert.Equal(expected, actual);
        }


        [Fact]
        public void TryCreate_WithValidPath_ReturnsTrue()
        {
            const string path = "C:\\tmp";
            var factory = new PurePathFactory();

            IPurePath tmp;
            var actual = factory.TryCreate(path, out tmp);

            Assert.True(actual);
        }

        [Fact]
        public void TryCreate_WithInvalidPath_ReturnsFalse()
        {
            const string path = ":\u0000::";
            var factory = new PurePathFactory();

            IPurePath tmp;
            var actual = factory.TryCreate(path, out tmp);

            Assert.False(actual);
        }

        [Fact]
        public void Create_FromMultiplePaths_CreatesPath()
        {
            var part1 = IsWindows ? "C:\\" : "C:/";
            const string part2 = "tmp";
            var factory = new PurePathFactory();
            var expected = IsWindows
                ? (IPurePath)new PureWindowsPath(part1, part2)
                : new PurePosixPath(part1, part2);

            var actual = factory.Create(part1, part2);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Create_WithNormCaseOption_CreatesPathAndNormalizesCase()
        {
            const string part1 = "C:\\TmP";
            var factory = new PurePathFactory();
            var expected = (IsWindows
                ? (IPurePath)new PureWindowsPath(part1)
                : new PurePosixPath(part1)).NormCase();

            var actual = factory.Create(part1);

            Assert.Equal(expected, actual);
        }
    }
}
