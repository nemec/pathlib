using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PathLib.UnitTest
{
    [TestClass]
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

        [TestMethod]
        public void Create_FromOnePath_CreatesPath()
        {
            const string part1 = "C:\\tmp";
            var factory = new PurePathFactory();
            var expected = IsWindows
                ? (IPurePath)new PureWindowsPath(part1)
                : new PurePosixPath(part1);

            var actual = factory.Create(part1);

            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void TryCreate_WithValidPath_ReturnsTrue()
        {
            const string path = "C:\\tmp";
            var factory = new PurePathFactory();

            IPurePath tmp;
            var actual = factory.TryCreate(path, out tmp);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void TryCreate_WithInvalidPath_ReturnsFalse()
        {
            const string path = ":\u0000::";
            var factory = new PurePathFactory();

            IPurePath tmp;
            var actual = factory.TryCreate(path, out tmp);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void Create_FromMultiplePaths_CreatesPath()
        {
            const string part1 = "C:\\";
            const string part2 = "tmp";
            var factory = new PurePathFactory();
            var expected = IsWindows
                ? (IPurePath)new PureWindowsPath(part1, part2)
                : new PurePosixPath(part1, part2);

            var actual = factory.Create(part1, part2);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Create_WithNormCaseOption_CreatesPathAndNormalizesCase()
        {
            const string part1 = "C:\\TmP";
            var factory = new PurePathFactory();
            var expected = (IsWindows
                ? (IPurePath)new PureWindowsPath(part1)
                : new PurePosixPath(part1)).NormCase();

            var actual = factory.Create(part1);

            Assert.AreEqual(expected, actual);
        }
    }
}
