
using System.Runtime.InteropServices;
using System.Text;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace PathLib.Posix
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Timespec
    {
        public long tv_sec;

        public ulong tv_nsec;

        public override string ToString()
        {
            return $"{tv_sec} / {tv_nsec}";
        }
    }

    // Get stat struct layout for platform
    // gcc -E /usr/include/x86_64-linux-gnu/bits/struct_stat.h
    // Get typedef mappings for struct fields
    // gcc -E /usr/include/x86_64-linux-gnu/sys/types.h
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct StatNative 
    {
        public ulong st_dev;
        public ulong st_ino;
        public ulong st_nlink;
        public uint st_mode;
        public uint st_uid;
        public uint st_gid;
        private int __pad0;
        public ulong st_rdev;
        public long st_size;
        public long st_blksize;
        public long st_blocks;

        public Timespec st_atim;
        public Timespec st_mtim;
        public Timespec st_ctim;

        private long __glibc_reserved0;
        private long __glibc_reserved2;
        private long __glibc_reserved3;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("st_dev ");
            sb.Append(st_dev);
            sb.AppendLine();
            sb.Append("st_ino ");
            sb.Append(st_ino);
            sb.AppendLine();
            sb.Append("st_mode ");
            sb.Append(st_mode);
            sb.AppendLine();
            sb.Append("st_nlink ");
            sb.Append(st_nlink);
            sb.AppendLine();
            sb.Append("st_uid ");
            sb.Append(st_uid);
            sb.AppendLine();

            sb.Append("st_gid ");
            sb.Append(st_gid);
            sb.AppendLine();

            sb.Append("st_rdev ");
            sb.Append(st_rdev);
            sb.AppendLine();

            sb.Append("st_size ");
            sb.Append(st_size);
            sb.AppendLine();

            sb.Append("st_blksize ");
            sb.Append(st_blksize);
            sb.AppendLine();

            sb.Append("st_blocks ");
            sb.Append(st_blocks);
            sb.AppendLine();

            sb.Append("st_atim ");
            sb.Append(st_atim);
            sb.AppendLine();

            sb.Append("st_mtim ");
            sb.Append(st_mtim);
            sb.AppendLine();

            sb.Append("st_ctim ");
            sb.Append(st_ctim);
            sb.AppendLine();


            return sb.ToString();
        }

    }

    public static class Native {
        [DllImport("libc", SetLastError = true, CharSet = CharSet.Auto, CallingConvention=CallingConvention.Cdecl)]
        public static extern int stat64(string path, out StatNative info);
        
    }
}