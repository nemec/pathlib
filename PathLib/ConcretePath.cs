using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace PathLib
{
    /// <summary>
    /// Base class for common methods in concrete paths.
    /// </summary>
    public abstract class ConcretePath : IPath
    {
        /// <inheritdoc/>
        protected readonly IPurePath PurePath;

        /// <inheritdoc/>
        protected ConcretePath(IPurePath purePath)
        {
            PurePath = purePath;
        }

        /// <inheritdoc/>
        public IEnumerable<IPath> Glob(string pattern)
        {
            if (!IsDir())
            {
                throw new ArgumentException("Glob may only be called on directories.");
            }
            throw new NotImplementedException();
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
        public bool IsDir()
        {
            return Directory.Exists(PurePath.AsPosix());
        }

        /// <inheritdoc/>
        public bool IsFile()
        {
            return File.Exists(PurePath.AsPosix());
        }

        /// <inheritdoc/>
        protected abstract IPath PathFactory(params string[] path);

        /// <inheritdoc/>
        public abstract IPath Resolve();

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
                Directory.CreateDirectory(PurePath.AsPosix());
            }
        }

        /// <inheritdoc/>
        public Stream Open(FileMode mode)
        {
            return File.Open(PurePath.AsPosix(), mode);
        }

        /// <summary>
        /// Path objects of the directory's contents.
        /// </summary>
        /// <exception cref="ConstraintException">
        /// Thrown if path does not represent a directory.
        /// </exception>
        /// <returns></returns>
        public IEnumerator<IPath> GetEnumerator()
        {
            if(!IsDir())
            {
                throw new ConstraintException("Path object must be a directory.");
            }
            foreach (var directory in Directory.GetFileSystemEntries(PurePath.ToString()))
            {
                yield return PathFactory(PurePath.Filename, directory);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
