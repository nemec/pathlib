
using System.Runtime.InteropServices;
using System.Text;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable InconsistentNaming
// ReSharper disable CommentTypo

namespace PathLib.Posix
{
    /* Test file sizes
        #include <stdio.h>
        #include <time.h>
        #include <sys/stat.h>
        #include <sys/types.h>

        int main()
        {
          printf ("Native type size check\n");
          printf ("stat %lu\n", sizeof(struct stat));
          printf ("dev_t %lu\n", sizeof(dev_t));
          printf ("ino_t %lu\n", sizeof(ino_t));
          printf ("mode_t %lu\n", sizeof(mode_t));
          printf ("nlink_t %lu\n", sizeof(nlink_t));
          printf ("uid_t %lu\n", sizeof(uid_t));
          printf ("gid_t %lu\n", sizeof(gid_t));
          printf ("blksize_t %lu\n", sizeof(blksize_t));
          printf ("blkcnt_t %lu\n", sizeof(blkcnt_t));
        }
     */
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
    // gcc -E /usr/include/x86_64-linux-gnu/sys/stat.h
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
        private readonly int __pad0;
        public ulong st_rdev;
        public long st_size;
        public long st_blksize;
        public long st_blocks;

        public Timespec st_atim;
        public Timespec st_mtim;
        public Timespec st_ctim;

        private readonly long __glibc_reserved0;
        private readonly long __glibc_reserved2;
        private readonly long __glibc_reserved3;

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

    internal static class Native {
        [DllImport("libc", SetLastError = true, CharSet = CharSet.Auto, CallingConvention=CallingConvention.Cdecl)]
        public static extern int stat64(string path, out StatNative info);
        
        [DllImport("libc", SetLastError = true, CharSet = CharSet.Auto, CallingConvention=CallingConvention.Cdecl)]
        public static extern int lstat(string path, out StatNative info);
        
        [DllImport("libc", SetLastError = true, CharSet = CharSet.Auto, CallingConvention=CallingConvention.Cdecl)]
        public static extern long readlink(string path, StringBuilder buf, ulong bufsize);

        /// <summary>
        /// S_IRUSR
        /// </summary>
        internal const uint UserRead = 0x100;// 0o400
        /// <summary>
        /// S_IWUSR
        /// </summary>
        internal const uint UserWrite = 0x80; // 0o200

        /// <summary>
        /// S_IXUSR
        /// </summary>
        internal const uint UserExecute = 0x40; //0o100
        /// <summary>
        /// S_IRWXU
        /// </summary>
        internal const uint UserBits = UserRead | UserWrite | UserExecute;  
        /// <summary>
        /// S_IRGRP
        /// </summary>
        internal const uint GroupRead = 0x20; // 0o40
        /// <summary>
        /// S_IWGRP
        /// </summary>
        internal const uint GroupWrite = 0x10; // 0o20
        /// <summary>
        /// S_IXGRP
        /// </summary>
        internal const uint GroupExecute = 0x8; // 0o10
        /// <summary>
        /// S_IRWXG
        /// </summary>
        internal const uint GroupBits = GroupRead | GroupWrite | GroupExecute;
        /// <summary>
        /// S_IROTH
        /// </summary>
        internal const uint OtherRead = 0x4; // 0o4
        /// <summary>
        /// S_IWOTH
        /// </summary>
        internal const uint OtherWrite = 0x2; // 0o2
        /// <summary>
        /// S_IXOTH
        /// </summary>
        internal const uint OtherExecute = 0x1; // 0o1
        /// <summary>
        /// S_IRWXO
        /// </summary>
        internal const uint OtherBits = OtherRead | OtherWrite | OtherExecute;
        /// <summary>
        /// S_ISVTX
        /// </summary>
        internal const uint Sticky = 0x200; // 0o1000
        /// <summary>
        /// S_ISUID
        /// </summary>
        internal const uint SetUID = 0x800; // 0o4000
        /// <summary>
        /// S_ISGID
        /// </summary>
        internal const uint SetGID = 0x400; // 0o2000

        internal const uint SetBits = Sticky | SetUID | SetGID;

        /// <summary>
        /// __S_IFDIR
        /// </summary>
        internal const uint FileTypeDirectory = 0x4000; // 0o040000
        /// <summary>
        /// __S_IFCHR
        /// </summary>
        internal const uint FileTypeCharacterDevice = 0x2000; // 0o020000
        /// <summary>
        /// __S_IFBLK
        /// </summary>
        internal const uint FileTypeBlockDevice = 0x6000; // 0o060000
        /// <summary>
        /// __S_IFREG
        /// </summary>
        internal const uint FileTypeRegularFile = 0x8000; // 0o100000
        /// <summary>
        /// __S_IFIFO
        /// </summary>
        internal const uint FileTypeFifo = 0x1000; // 0o010000
        /// <summary>
        /// __S_IFLNK
        /// /usr/include/x86_64-linux-gnu/bits/stat.h
        /// </summary>
        internal const uint FileTypeSymlink = 0xa000; // 0o120000
        /// <summary>
        /// __S_IFSOCK
        /// </summary>
        internal const uint FileTypeSocket = 0xc000; // 0o140000

        /// <summary>
        /// The bit mask for all file types.
        /// </summary>
        internal const uint FileTypeFormat = 0xf000; // 0o170000

    }
}