using System;
using System.ComponentModel;
using Xunit;

namespace PathLib.UnitTest
{
    public class PathUnitTest
    {
        [Fact]
        public void Create_WithTypeConverter_CreatesPathForPlatform()
        {
            var isWindows = true;
            var p = Environment.OSVersion.Platform;
            // http://mono.wikia.com/wiki/Detecting_the_execution_platform
            // 128 required for early versions of Mono
            if (p == PlatformID.Unix || p == PlatformID.MacOSX || (int)p == 128)
            {
                isWindows = false;
            }

            const string path = @"C:\users\tmp";
            var converter = TypeDescriptor.GetConverter(typeof (IPath));
            var expected = isWindows
                ? typeof (WindowsPath)
                : typeof (PosixPath);

            var actual = converter.ConvertFromInvariantString(path);

            Assert.IsType(expected, actual);
        }
    }
}
