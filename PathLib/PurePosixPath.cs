using System;

namespace PathLib
{
    /// <summary>
    /// Represents a POSIX path. Uses the slash as a directory separator
    /// and treats paths as case sensitive.
    /// </summary>
    public class PurePosixPath : PurePath<PurePosixPath>, IEquatable<PurePosixPath>
    {
        #region ctors

        /// <summary>
        /// Create a path in the current working directory.
        /// </summary>
        public PurePosixPath()
        {
            Initialize();
        }

        /// <summary>
        /// Create a path by joining the given path strings.
        /// </summary>
        /// <param name="paths">Paths to combine.</param>
        public PurePosixPath(params string[] paths)
            : base(paths)
        {
            Initialize();
        }

        /// <summary>
        /// Create a path by joinin the given IPurePaths.
        /// </summary>
        /// <param name="paths">Paths to combine.</param>
        public PurePosixPath(params IPurePath[] paths)
            : base(paths)
        {
            Initialize();
        }

        /// <inheritdoc/>
        protected PurePosixPath(string drive, string root, string dirname, string basename, string extension)
            : base(drive, root, dirname, basename, extension)
        {
            Initialize();
        }

        #endregion

        private void Initialize()
        {
            var path = RawPath;

            if (path == null)
            {
                return;
            }

            Drive = String.Empty;

            // Special case in POSIX pathname resolution
            // http://pubs.opengroup.org/onlinepubs/009695399/basedefs/xbd_chap04.html#tag_04_11
            if (path.StartsWith(PathSeparator + PathSeparator) &&
                (path.Length <= 2 || ("" + path[2]) != PathSeparator))
            {
                Root = PathSeparator + PathSeparator;
            }
            else if (path.StartsWith(PathSeparator))
            {
                Root = PathSeparator;
            }
            else
            {
                Root = String.Empty;
            }

            if (Drive.Length + Root.Length >= path.Length)
            {
                return;
            }

            path = path.Substring(Drive.Length + Root.Length);

            // Remove trailing slash
            // This is what Python's pathlib does, but I don't think it's
            // necessarily required by spec
            if (path.EndsWith(PathSeparator))
            {
                path = path.TrimEnd(PathSeparator.ToCharArray());
            }

            Dirname = ParseDirname(path);
            path = path.Substring(Dirname.Length);

            Basename = ParseBasename(path);
            path = path.Substring(Basename.Length);

            Extension = ParseExtension(path);

            Normalize(this);
        }

        /// <inheritdoc/>
        protected override PurePosixPath PurePathFactory(string path)
        {
            return new PurePosixPath(path);
        }

        /// <inheritdoc/>
        protected override PurePosixPath PurePathFactoryFromComponents(string drive, string root, string dirname, string basename, string extension)
        {
            return new PurePosixPath(drive, root, dirname, basename, extension);
        }

        /// <inheritdoc/>
        protected override string PathSeparator
        {
            get { return "/"; }
        }

        #region Parsing Initializers

        private string ParseDirname(string remainingPath)
        {
            // Hardcode special dirs
            if (remainingPath == "." || remainingPath == "..")
            {
                return remainingPath;
            }
            var idx = remainingPath.LastIndexOf(PathSeparator,
                StringComparison.CurrentCulture);
            return idx > 1
                ? remainingPath.Substring(0, idx + 1)
                : "";
        }

        private string ParseBasename(string remainingPath)
        {
            return !String.IsNullOrEmpty(remainingPath)
                ? remainingPath != "."  // Special case for current dir.
                    ? PathUtils.GetFileNameWithoutExtension(remainingPath, PathSeparator)
                    : "."
                : "";
        }

        private string ParseExtension(string remainingPath)
        {
            return !String.IsNullOrEmpty(remainingPath)
                ? PathUtils.GetExtension(remainingPath, PathSeparator)
                : "";
        }

        #endregion

        #region Equality Members

        /// <summary>
        /// Compare two <see cref="PurePosixPath"/> for equality.
        /// Case sensitive.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator ==(PurePosixPath first, PurePosixPath second)
        {
            if (ReferenceEquals(first, null) ||
                ReferenceEquals(second, null))
            {
                return ReferenceEquals(first, second);
            }
            return first.Equals(second);
        }

        /// <summary>
        /// Compare two <see cref="PurePosixPath"/> for inequality.
        /// Case sensitive.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator !=(PurePosixPath first, PurePosixPath second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Return true if <paramref name="first"/> is a parent path
        /// of <paramref name="second"/>.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator <(PurePosixPath first, PurePosixPath second)
        {
            if (first == null || second == null)
            {
                return false;
            }

            if (LinqBridge.Count(first.Parts) >= LinqBridge.Count(second.Parts))
            {
                return false;
            }

            foreach (var parts in LinqBridge.Zip(first.Parts, second.Parts, (p, c) => new[]{p, c}))
            {
                if (parts[0] != parts[1])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Return true if <paramref name="second"/> is a parent of
        /// <paramref name="first"/>.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator >(PurePosixPath first, PurePosixPath second)
        {
            return second < first;
        }

        /// <summary>
        /// Return true if <paramref name="first"/> is equal to or a parent
        /// of <paramref name="second"/>.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator <=(PurePosixPath first, PurePosixPath second)
        {
            return first == second || first < second;
        }

        /// <summary>
        /// Return true if <paramref name="second"/> is equal to or a parent
        /// of <paramref name="first"/>.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator >=(PurePosixPath first, PurePosixPath second)
        {
            return first == second || first > second;
        }

        /// <summary>
        /// Compare two <see cref="PurePosixPath"/> for equality.
        /// Case sensitive.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(PurePosixPath other)
        {
            return other != null && AsPosix().Equals(other.AsPosix());
        }

        /// <summary>
        /// Compare two <see cref="PurePosixPath"/> for equality.
        /// Case sensitive.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(object other)
        {
            var obj = other as PurePosixPath;
            return obj != null && Equals(obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return AsPosix().GetHashCode();
        }

        #endregion

        #region Operators

        /// <summary>
        /// Join two <see cref="PurePosixPath"/>s.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static PurePosixPath operator +(PurePosixPath first, PurePosixPath second)
        {
            return first.Join(second) as PurePosixPath;
        }

        /// <summary>
        /// Join a <see cref="PurePosixPath"/> with a string.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static PurePosixPath operator +(PurePosixPath first, string second)
        {
            return first.Join(second) as PurePosixPath;
        }

        #endregion

        /// <inheritdoc/>
        public override bool IsReserved()
        {
            return false;
        }

        /// <inheritdoc/>
        public override bool Match(string pattern)
        {
            return PathUtils.Glob(pattern, AsPosix(), IsAbsolute());
        }

        /// <inheritdoc/>
        public override PurePosixPath NormCase()
        {
            return this;
        }
    }
}
