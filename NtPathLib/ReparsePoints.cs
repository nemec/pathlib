using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace PathLib
{
    internal class ReparsePoints
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        // The CharSet must match the CharSet of the corresponding PInvoke signature
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct WIN32_FIND_DATA
        {
            public uint dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        public uint FILE_ATTRIBUTE_REPARSE_POINT = 1024;  // swFileAttributes

        public uint IO_REPARSE_TAG_SYMLINK = 0xA000000C;  // dwReservedFile0
        public uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;  // dwReservedFile0

    }
}
