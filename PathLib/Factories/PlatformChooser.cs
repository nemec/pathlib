
using System;

namespace PathLib
{
    internal enum Platform
    {
        Windows,
        Posix
    }

    internal static class PlatformChooser
    {
        public static Platform GetPlatform()
        {
            var p = Environment.OSVersion.Platform;
            // http://mono.wikia.com/wiki/Detecting_the_execution_platform
            // 128 required for early versions of Mono
            if (p == PlatformID.Unix || p == PlatformID.MacOSX || (int)p == 128)
            {
                return Platform.Posix;
            }
            return Platform.Windows;
        }
    }
}
