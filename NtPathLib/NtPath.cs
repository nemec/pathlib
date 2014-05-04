using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace PathLib
{
    public class NtPath : ConcretePath
    {
        public NtPath(params string[] paths)
            : base(new PureNtPath(paths))
        {
        }

        /// <summary>
        /// TODO
        /// If a file name begins with only a disk designator 
        /// but not the backslash after the colon, it is 
        /// interpreted as a relative path to the current 
        /// directory on the drive with the specified letter. 
        /// Note that the current directory may or may not be 
        /// the root directory depending on what it was set to 
        /// during the most recent "change directory" operation 
        /// on that disk.
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa365247(v=vs.85).aspx
        /// </summary>
        private IPath _cachedResolve;
        public override IPath Resolve()
        {
            if (_cachedResolve != null) return _cachedResolve;

            var parts = new Stack<string>();
            foreach (var part in PurePath.Parts)
            {
                // TODO join parts and check for symlink
                if (part == String.Format("{0}{0}", PathUtils.CurrentDirectoryIdentifier))
                {
                    parts.Pop();
                    continue;
                }
                parts.Push(part);
            }
            return (_cachedResolve = new NtPath(parts.ToArray()));
        }

        protected override IPath PathFactory(params string[] paths)
        {
            return new NtPath(paths);
        }

        private StatInfo _cachedStat;
        protected override StatInfo Stat(bool flushCache)
        {
            if(_cachedStat != null && !flushCache)
            {
                return _cachedStat;
            }

            // http://www.delorie.com/gnu/docs/glibc/libc_284.html
            var info = new FileInfo(PurePath.AsPosix());
            var stat = new StatInfo
            {
                Size = info.Length,
                ATime = info.LastAccessTimeUtc,
                MTime = info.LastWriteTimeUtc,
                CTime = info.CreationTimeUtc,
                Device = 0,
                Inode = 0,
                Gid = 0,
                Uid = 0,
                Mode = 0 // TODO not implemented
            };

            _cachedStat = stat;
            return stat;
        }

        public override bool IsSymlink()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return a path that uses the short "8.3" form of the
        /// filename. (e.g. DOCUME~2.docx) 
        /// </summary>
        /// <returns></returns>
        public NtPath ToShortPath()
        {
            var oldPath = PurePath.ToString();
            var newPath = new StringBuilder(255);
            if (GetShortPathName(oldPath, newPath, newPath.Capacity) == 0)
            {
                return this;
            }
            return new NtPath(PurePath.WithFilename(newPath.ToString()).ToString());
        }

        /// <summary>
        /// Convert a path that uses the short 8.3 name (e.g. DOCUME~2.docx)
        /// into its long path.
        /// </summary>
        /// <returns></returns>
        public NtPath ToLongPath()
        {
            var oldPath = PurePath.ToString();
            var newPath = new StringBuilder(255);
            if (GetLongPathName(oldPath, newPath, newPath.Capacity) == 0)
            {
                return this;
            }
            return new NtPath(PurePath.WithFilename(newPath.ToString()).ToString());
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetLongPathName(
            [MarshalAs(UnmanagedType.LPStr)] string path,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder longPath,
            int longPathLength);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetShortPathName(
            [MarshalAs(UnmanagedType.LPStr)] string path,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder shortPath,
            int longPathLength);
    }
}
