using System;
using System.Collections.Generic;
using System.IO;

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
                if (part == String.Format("{0}{0}", PathLib.PurePath.CurrentDirectoryIdentifier))
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
    }
}
