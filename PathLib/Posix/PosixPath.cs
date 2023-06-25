using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using PathLib.Posix;

// ReSharper disable once CheckNamespace
namespace PathLib
{
    /// <summary>
    /// Concrete path implementation for Posix machines (Linux, Unix, Mac).
    /// Unusable on other systems.
    /// </summary>
    public sealed class PosixPath : ConcretePath<PosixPath, PurePosixPath>, IEquatable<PosixPath>
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
                atim = atim.AddTicks((long)(info.st_atim.tv_nsec / 100));
            }
            var mtim = epoch.AddSeconds(info.st_mtim.tv_sec);
            if (info.st_mtim.tv_nsec != 0)
            {
                mtim = mtim.AddTicks((long)(info.st_mtim.tv_nsec / 100));
            }
            var ctim = epoch.AddSeconds(info.st_ctim.tv_sec);
            if (info.st_ctim.tv_nsec != 0)
            {
                ctim = ctim.AddTicks((long)(info.st_ctim.tv_nsec / 100));
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
                ModeDecimal = info.st_mode,
                Mode = Convert.ToString(info.st_mode & (Native.SetBits | Native.UserBits | Native.GroupBits | Native.OtherBits), 8).PadLeft(4, '0'),
                NumLinks = (long)info.st_nlink
            };
            try
            {
                _cachedStat = stat;
            }
            // Yes, this assignment throws a NRE if the struct alignment for pinvoke is wrong.
            // It overwrites 'this' in the stack with null.
            catch (NullReferenceException e)
            {
                throw new NotImplementedException("Layout of stat call not supported on this platform (Only supports Ubuntu x86_64 and anything compatible).", e);
            }
            return stat;
        }

        /// <inheritdoc/>
        protected override PosixPath PathFactory(params string[] path)
        {
            if (path == null)
            {
                throw new NullReferenceException("Path passed to factory cannot be null");
            }

            return new PosixPath(path);
        }

        /// <inheritdoc/>
        protected override PosixPath PathFactory(PurePosixPath path)
        {
            if (path == null)
            {
                throw new NullReferenceException("Path passed to factory cannot be null");
            }

            return new PosixPath(path);
        }

        /// <inheritdoc/>
        protected override PosixPath PathFactory(IPurePath path)
        {
            if (path == null)
            {
                throw new NullReferenceException("Path passed to factory cannot be null");
            }

            var purePath = path as PurePosixPath;
            if (purePath != null)
            {
                return new PosixPath(purePath);
            }

            var parts = new List<string>();
            parts.AddRange(path.Parts);
            return new PosixPath(parts.ToArray());
        }

        /// <inheritdoc/>
        public override PosixPath Resolve()
        {
            var path = ExpandUser().ExpandEnvironmentVars().ToString();
            var realpath = Native.realpath(path) ?? throw new ApplicationException($"Resolve failed: realpath returned null");
            return new PosixPath(realpath);
        }

        /// <inheritdoc/>
        public override bool IsSymlink()
        {
            var fileType = GetFileType();
            return fileType == FileType.SymbolicLink;
        }

        /// <summary>
        /// Gets the type of the file in the filesystem. This is
        /// unrelated to the extension/contents, rather it is an
        /// enumeration between whether the path points to a directory,
        /// file, symlink, socket, block device, etc.
        /// This is currently only relevant to Posix/Linux systems.
        /// </summary>
        /// <returns></returns>
        public FileType GetFileType()
        {
            var path = PurePath.ToString();
            var err = Native.lstat(path, out var info);
            if(err != 0)
            {
                var actualError = Marshal.GetLastWin32Error();
                if (actualError == 2)
                {
                    return FileType.DoesNotExist;
                }
                throw new ApplicationException("Error: " + actualError);
            }

            var fmt = info.st_mode & Native.FileTypeFormat;
            return fmt switch
            {
                Native.FileTypeSymlink => FileType.SymbolicLink,
                Native.FileTypeDirectory => FileType.Directory,
                Native.FileTypeCharacterDevice => FileType.CharacterDevice,
                Native.FileTypeBlockDevice => FileType.BlockDevice,
                Native.FileTypeFifo => FileType.Fifo,
                Native.FileTypeSocket => FileType.Socket,
                Native.FileTypeRegularFile => FileType.RegularFile,
                _ => throw new ApplicationException($"Unknown file type {fmt}")
            };
        }

        /// <inheritdoc/>
        public override PosixPath ExpandUser()
        {
            var homeDir = new PurePosixPath("~");
            if (homeDir < PurePath)
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (!String.IsNullOrEmpty(home))
                {
                    return new PosixPath(
                        new PurePosixPath(home).Join(PurePath.RelativeTo(homeDir)));
                }

                throw new ApplicationException("Unable to find home directory for user");
            }

            return this;
        }

        /// <inheritdoc/>
        public override IEnumerable<DirectoryContents<PosixPath>> WalkDir(Action<IOException> onError = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool Equals(PosixPath other)
        {
            if (other is null) return false;
            return PurePath.Equals(other.PurePath);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return PurePath.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return PurePath.ToString();
        }
    }
}
