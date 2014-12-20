using System;

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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a new path object for Posix-compliant machines.
        /// </summary>
        /// <param name="path"></param>
        public PosixPath(PurePosixPath path)
            : base(path)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override StatInfo Stat(bool flushCache)
        {
            throw new NotImplementedException();
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
    }
}
