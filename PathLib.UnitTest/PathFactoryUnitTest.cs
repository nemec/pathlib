﻿using System;
using System.Runtime.InteropServices;
using Xunit;

namespace PathLib.UnitTest
{
    public sealed class IgnoreOnLinuxFact : FactAttribute
    {
        public IgnoreOnLinuxFact() {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                Skip = "Ignore on Linux";
            }
        }
    }
    
    public class PathFactoryUnitTest
    {
        private static readonly bool IsWindows;

        static PathFactoryUnitTest()
        {
            var p = Environment.OSVersion.Platform;
            // http://mono.wikia.com/wiki/Detecting_the_execution_platform
            // 128 required for early versions of Mono
            if (p == PlatformID.Unix || p == PlatformID.MacOSX || (int)p == 128)
            {
                IsWindows = false;
                return;
            }

            IsWindows = true;
        }

        [Fact]
        public void Create_FromOnePath_CreatesPath()
        {
            const string part1 = "C:\\tmp";
            var factory = new PathFactory();
            var expected = IsWindows
                ? (IPath)new WindowsPath(part1)
                : new PosixPath(part1);

            var actual = factory.Create(part1);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Create_FromMultiplePaths_CreatesPath()
        {
            const string part1 = "C:\\";
            const string part2 = "tmp";
            var factory = new PathFactory();
            var expected = IsWindows
                ? (IPath)new WindowsPath(part1, part2)
                : new PosixPath(part1, part2);

            var actual = factory.Create(part1, part2);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryCreate_WithValidPath_ReturnsTrue()
        {
            const string path = "C:\\tmp";
            var factory = new PathFactory();
            
            IPath tmp;
            var actual = factory.TryCreate(path, out tmp);

            Assert.True(actual);
        }

        [IgnoreOnLinuxFact]
        public void TryCreate_WithInvalidPath_ReturnsFalse()
        {
            const string path = ":::";
            var factory = new PathFactory();

            IPath tmp;
            var actual = factory.TryCreate(path, out tmp);

            Assert.False(actual);
        }

        [Fact]
        public void Create_WithNormCaseOption_CreatesPathAndNormalizesCase()
        {
            const string part1 = "C:\\TmP";
            var factory = new PathFactory();
            var expected = (IsWindows
                ? (IPath)new WindowsPath(part1)
                : new PosixPath(part1)).NormCase();

            var actual = factory.Create(part1);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Create_WithExpandEnvVarsOption_CreatesPathExpandsVars()
        {
            Environment.SetEnvironmentVariable("MYCUSTOMBIN", "bin");
            const string expectedDir = @"C:\bin\myprogram.exe";
            const string unexpandedDir = @"C:\%MYCUSTOMBIN%\myprogram.exe";
            var factory = new PathFactory();
            var expected = (IsWindows
                ? (IPath)new WindowsPath(expectedDir)
                : new PosixPath(expectedDir)).NormCase();
            var options = new PathFactoryOptions
            {
                AutoExpandEnvironmentVariables = true
            };

            var actual = factory.Create(unexpandedDir, options);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Create_WithExpandUserOption_CreatesPathAndExpandsUser()
        {
            var userDir = IsWindows
                ? Environment.GetEnvironmentVariable("USERPROFILE")
                : Environment.GetEnvironmentVariable("HOME");
            var expectedDir = userDir + @"\tmp";
            const string unexpandedDir = @"~\tmp";
            var factory = new PathFactory();
            var expected = (IsWindows
                ? (IPath)new WindowsPath(expectedDir)
                : new PosixPath(expectedDir)).NormCase();
            var options = new PathFactoryOptions
            {
                AutoExpandUserDirectory = true
            };

            var actual = factory.Create(unexpandedDir, options);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Create_WithExpandUserOptionAndExplicitUserDirectory_CreatesPathAndExpandsUser()
        {
            const string expectedDir = @"C:\users\fake\tmp";
            const string unexpandedDir = @"~\tmp";
            var factory = new PathFactory();
            var expected = (IsWindows
                ? (IPath)new WindowsPath(expectedDir)
                : new PosixPath(expectedDir)).NormCase();
            var options = new PathFactoryOptions
            {
                AutoExpandUserDirectory = true,
                UserDirectory = factory.Create(@"C:\users\fake")
            };

            var actual = factory.Create(unexpandedDir, options);

            Assert.Equal(expected, actual);
        }
    }
}
