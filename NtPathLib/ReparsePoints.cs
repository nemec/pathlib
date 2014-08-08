using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

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

    /// <remarks>
    /// Refer to http://msdn.microsoft.com/en-us/library/windows/hardware/ff552012%28v=vs.85%29.aspx
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct SymbolicLinkReparseData
    {
        // Not certain about this!
        private const int maxUnicodePathLength = 260 * 2;

        public uint ReparseTag;
        public ushort ReparseDataLength;
        public ushort Reserved;
        public ushort SubstituteNameOffset;
        public ushort SubstituteNameLength;
        public ushort PrintNameOffset;
        public ushort PrintNameLength;
        public uint Flags;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = maxUnicodePathLength)]
        public byte[] PathBuffer;
    }

    public static class SymbolicLink
    {
        private const uint genericReadAccess = 0x80000000;

        private const uint fileFlagsForOpenReparsePointAndBackupSemantics = 0x02200000;

        private const int ioctlCommandGetReparsePoint = 0x000900A8;

        private const uint openExisting = 0x3;

        private const uint pathNotAReparsePointError = 0x80071126;

        private const uint shareModeAll = 0x7; // Read, Write, Delete

        private const uint symLinkTag = 0xA000000C;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            int nInBufferSize,
            IntPtr lpOutBuffer,
            int nOutBufferSize,
            out int lpBytesReturned,
            IntPtr lpOverlapped);


        public static bool Exists(string path)
        {
            if (!Directory.Exists(path) && !File.Exists(path))
            {
                return false;
            }
            var target = GetTarget(path);
            return target != null;
        }

        private static SafeFileHandle getFileHandle(string path)
        {
            return CreateFile(path, genericReadAccess, shareModeAll, IntPtr.Zero, openExisting,
                fileFlagsForOpenReparsePointAndBackupSemantics, IntPtr.Zero);
        }

        public static string GetTarget(string path)
        {
            SymbolicLinkReparseData reparseDataBuffer;

            using (var fileHandle = getFileHandle(path))
            {
                if (fileHandle.IsInvalid)
                {
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }

                var outBufferSize = Marshal.SizeOf(typeof(SymbolicLinkReparseData));
                var outBuffer = IntPtr.Zero;
                try
                {
                    outBuffer = Marshal.AllocHGlobal(outBufferSize);
                    int bytesReturned;
                    bool success = DeviceIoControl(
                        fileHandle.DangerousGetHandle(), ioctlCommandGetReparsePoint, IntPtr.Zero, 0,
                        outBuffer, outBufferSize, out bytesReturned, IntPtr.Zero);

                    fileHandle.Close();

                    if (!success)
                    {
                        if (((uint)Marshal.GetHRForLastWin32Error()) == pathNotAReparsePointError)
                        {
                            return null;
                        }
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                    }

                    reparseDataBuffer = (SymbolicLinkReparseData)Marshal.PtrToStructure(
                        outBuffer, typeof(SymbolicLinkReparseData));
                }
                finally
                {
                    Marshal.FreeHGlobal(outBuffer);
                }
            }
            if (reparseDataBuffer.ReparseTag != symLinkTag)
            {
                return null;
            }

            var target = Encoding.Unicode.GetString(reparseDataBuffer.PathBuffer,
                reparseDataBuffer.PrintNameOffset, reparseDataBuffer.PrintNameLength);

            return target;
        }
    }

}
