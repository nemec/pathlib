using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using PathLib.Utils;

namespace PathLib
{
    /// <summary>
    /// Base class for common methods in concrete paths.
    /// </summary>
    public abstract class ConcretePath<TPath, TPurePath> : IPath<TPath>
        where TPath : IPath
        where TPurePath : IPurePath
    {
        /// <inheritdoc/>
        public readonly TPurePath PurePath;

        /// <inheritdoc/>
        protected ConcretePath(TPurePath purePath)
        {
            PurePath = purePath;
        }

        /// <inheritdoc/>
        public IEnumerable<TPath> ListDir(string pattern)
        {
            if (!IsDir())
            {
                throw new ArgumentException("Glob may only be called on directories.");
            }
            foreach (var path in ListDir())
            {
                if (PathUtils.Glob(pattern, path.ToString()))
                {
                    yield return path;
                }
            }
        }

        IEnumerable<IPath> IPath.ListDir(string pattern)
        {
            return LinqBridge.Select(ListDir(pattern), p => (IPath)p);
        }

        /// <inheritdoc/>
        protected abstract StatInfo Stat(bool flushCache);

        /// <inheritdoc/>
        public StatInfo Stat()
        {
            return Stat(false);
        }

        /// <inheritdoc/>
        public StatInfo Restat()
        {
            return Stat(true);
        }

        /// <inheritdoc/>
        public void Chmod(int mode)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool Exists()
        {
            return IsDir() || IsFile();
        }

        /// <inheritdoc/>
        public bool IsFile()
        {
            return File.Exists(PurePath.ToPosix());
        }

        /// <inheritdoc/>
        public bool IsDir()
        {
            return Directory.Exists(PurePath.ToPosix());
        }

        /// <inheritdoc/>
        public IEnumerable<TPath> ListDir()
        {
            if (!IsDir())
            {
                throw new ConstraintException("Path object must be a directory.");
            }
            foreach (var directory in Directory.GetFileSystemEntries(PurePath.ToString()))
            {
                yield return PathFactory(PurePath.Filename, directory);
            }
        }

        /// <inheritdoc/>
        IEnumerable<IPath> IPath.ListDir()
        {
            return LinqBridge.Select(ListDir(), p => (IPath)p);
        }

        /// <inheritdoc/>
        protected abstract TPath PathFactory(params string[] path);

        /// <inheritdoc/>
        public abstract TPath Resolve();

        IPath IPath.Resolve()
        {
            return Resolve();
        }

        /// <inheritdoc/>
        public abstract bool IsSymlink();

        /// <inheritdoc/>
        public void Lchmod(int mode)
        {
            Resolve().Chmod(mode);
        }

        /// <inheritdoc/>
        public StatInfo Lstat()
        {
            return Resolve().Stat();
        }

        /// <inheritdoc/>
        public void Mkdir(bool makeParents = false)
        {
            // Iteratively check whether or not each directory in the path exists
            // and create them if they do not.
            if (makeParents)
            {
                foreach (var dir in PurePath.Parents())
                {
                    if(!PathFactory(dir.ToString()).IsDir())
                    {
                        Directory.CreateDirectory(dir.ToString());
                    }
                }
            }
            if (!IsDir())
            {
                Directory.CreateDirectory(PurePath.ToPosix());
            }
        }

        /// <inheritdoc/>
        public Stream Open(FileMode mode)
        {
            return File.Open(PurePath.ToString(), mode);
        }

        /// <inheritdoc/>
        public abstract TPath ExpandUser();

        IPath IPath.ExpandUser()
        {
            return ExpandUser();
        }

        /// <inheritdoc/>
        public abstract IDisposable SetCurrentDirectory();
    }
}
