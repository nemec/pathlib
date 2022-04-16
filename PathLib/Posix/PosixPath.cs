using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using PathLib.Posix;

namespace PathLib
{
    /// <summary>
    /// Concrete path implementation for Posix machines (Linux, Unix, Mac).
    /// Unusable on other systems.
    /// </summary>
    public class PosixPath : ConcretePath<PosixPath, PurePosixPath>
    {
        /// <summary>
        /// Create a new path object for Posix-compliant machines.
        /// </summary>
        /// <param name="paths"></param>
        public PosixPath(params string[] paths)
            : base(new PurePosixPath(paths))
        {
        }

        /// <summary>
        /// Create a new path object for Posix-compliant machines.
        /// </summary>
        /// <param name="path"></param>
        public PosixPath(PurePosixPath path)
            : base(path)
        {
        }

        private StatInfo _cachedStat;
        /// <inheritdoc/>
        protected override StatInfo Stat(bool flushCache)
        {
            if(_cachedStat != null && !flushCache)
            {
                return _cachedStat;
            }

            var path = PurePath.ToString();
            var err = Posix.Native.stat64(path, out var info);
            if(err != 0)
            {
                var actualError = Marshal.GetLastWin32Error();
                if (actualError == 2)
                {
                    throw new FileNotFoundException("Cannot stat file that does not exist.", path);
                }
                throw new ApplicationException("Error: " + actualError);
            }

            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var atim = epoch.AddSeconds(info.st_atim.tv_sec);
            if (info.st_atim.tv_nsec != 0)
            {
                atim = atim.AddMilliseconds(info.st_atim.tv_nsec / 100000.0);
            }
            var mtim = epoch.AddSeconds(info.st_mtim.tv_sec);
            if (info.st_mtim.tv_nsec != 0)
            {
                mtim = mtim.AddMilliseconds(info.st_mtim.tv_nsec / 100000.0);
            }
            var ctim = epoch.AddSeconds(info.st_ctim.tv_sec);
            if (info.st_ctim.tv_nsec != 0)
            {
                ctim = ctim.AddMilliseconds(info.st_ctim.tv_nsec / 100000.0);
            }
            var stat = new StatInfo
            {
                Size = info.st_size,
                ATime = atim,
                MTime = mtim,
                CTime = ctim,
                Device = (long)info.st_dev,
                Inode = (long)info.st_ino,
                Gid = info.st_uid,
                Uid = info.st_gid,
                Mode = (int)info.st_mode
            };
            try
            {
                _cachedStat = stat;
            }
            // Yes, this assignment throws a NRE if the struct alignment for pinvoke is wrong. 
            catch (NullReferenceException e)
            {
                throw new NotImplementedException("Layout of stat call not supported on this platform (Only supports Ubuntu x86_64 and anything compatible).", e);
            }
            return stat;
        }

        /// <inheritdoc/>
        protected override PosixPath PathFactory(params string[] path)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override PosixPath PathFactory(PurePosixPath path)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override PosixPath PathFactory(IPurePath path)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override PosixPath Resolve()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override bool IsSymlink()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override PosixPath ExpandUser()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override IDisposable SetCurrentDirectory()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override IEnumerable<DirectoryContents<PosixPath>> WalkDir(Action<IOException> onError = null)
        {
            throw new NotImplementedException();
        }
    }
}
