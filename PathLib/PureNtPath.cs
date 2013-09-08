using System;
using System.IO;

namespace PathLib
{
    /// <summary>
    /// Represents an NT path. Uses the backslash for a separator and
    /// treats paths as case insensitive.
    /// </summary>
    public class PureNtPath : PurePath, IEquatable<PureNtPath>
    {
        private readonly string[] _reservedPaths = new[]
            {
                "CON", "PRN", "AUX", "NUL",
                "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
                "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
            };

        #region ctors

        /// <summary>
        /// Create an NT path in the current working directory.
        /// Uses the backslash for a separator and treats paths as
        /// case insensitive.
        /// </summary>
        public PureNtPath()
        {
            Initialize();
        }

        /// <summary>
        /// Create an NT path by joining the given path strings.
        /// Uses the backslash for a separator and treats paths as
        /// case insensitive.
        /// </summary>
        /// <param name="paths">Paths to combine.</param>
        public PureNtPath(params string[] paths)
            : base(paths)
        {
            Initialize();
        }

        /// <summary>
        /// Create an NT path by joining the given IPurePaths.
        /// Uses the backslash for a separator and treats paths as
        /// case insensitive.
        /// </summary>
        /// <param name="paths">Paths to combine.</param>
        public PureNtPath(params IPurePath[] paths)
            : base(paths)
        {
            Initialize();
        }

        /// <summary>
        /// Create an NT path with the given components.
        /// Uses the backslash for a separator and treats paths as
        /// case insensitive.
        /// </summary>
        /// <param name="drive"></param>
        /// <param name="root"></param>
        /// <param name="dirname"></param>
        /// <param name="basename"></param>
        /// <param name="extension"></param>
        protected PureNtPath(string drive, string root, string dirname, string basename, string extension)
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
                Normalize(this);
                return;
            }

            Drive = ParseDrive(path);
            Root = ParseRoot(path);

            if (Drive.Length + Root.Length >= path.Length)
            {
                return;
            }

            path = path.Substring(Drive.Length + Root.Length);

            Dirname = ParseDirname(path);
            path = path.Substring(Dirname.Length);

            // Remove trailing slash
            // This is what Python's pathlib does, but I don't think it's
            // necessarily required by spec
            if (Dirname.EndsWith(PathSeparator))
            {
                Dirname = Dirname.Substring(0, Dirname.Length - 1);
            }

            Basename = ParseBasename(path);
            path = path.Substring(Basename.Length);

            Extension = ParseExtension(path);

            Normalize(this);
        }

        /// <inheritdoc/>
        protected override IPurePath PurePathFactory(string path)
        {
            return new PureNtPath(path);
        }

        /// <inheritdoc/>
        protected override string PathSeparator
        {
            get { return @"\"; }
        }

        #region Parsing Initializers

        private string ParseDrive(string path)
        {
            return String.IsNullOrEmpty(path)
                ? String.Empty
                : Path.GetPathRoot(path).TrimEnd(PathSeparator[0]);
        }

        private string ParseRoot(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return String.Empty;
            }

            var root = Path.GetPathRoot(path);
            if (root.StartsWith(PathSeparator))
            {
                return PathSeparator;
            }
            return root.EndsWith(PathSeparator)
                       ? PathSeparator
                       : "";
        }

        private static string ParseDirname(string remainingPath)
        {
            return Path.GetDirectoryName(remainingPath) ?? "";
        }

        private static string ParseBasename(string remainingPath)
        {
            return !String.IsNullOrEmpty(remainingPath)
                ? Path.GetFileNameWithoutExtension(remainingPath)
                : "";
        }

        private static string ParseExtension(string remainingPath)
        {
            return !String.IsNullOrEmpty(remainingPath)
                ? Path.GetExtension(remainingPath)
                : "";
        }

        #endregion

        #region Equality Members

        /// <summary>
        /// Compare two <see cref="PureNtPath"/> for equality.
        /// Case insensitive.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator ==(PureNtPath first, PureNtPath second)
        {
            return ReferenceEquals(first, null) ?
                ReferenceEquals(second, null) :
                first.Equals(second);
        }

        /// <summary>
        /// Compare two <see cref="PureNtPath"/> for inequality.
        /// Case insensitive.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator !=(PureNtPath first, PureNtPath second)
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
        public static bool operator <(PureNtPath first, PureNtPath second)
        {
            if (first == null || second == null)
            {
                return false;
            }

            // Resolve symlinks before comparing
            var parent = LinqBridge.ToArray(first.NormCase().Parts);
            var child = LinqBridge.ToArray(second.NormCase().Parts);

            // Parent must be shorter than child
            if (LinqBridge.Count(parent) >= LinqBridge.Count(child))
            {
                return false;
            }
            foreach (var parts in LinqBridge.Zip(parent, child, (p, c) => new [] {p, c}))
            {
                if (parts[0].ToLowerInvariant() != parts[1].ToLowerInvariant())
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
        public static bool operator >(PureNtPath first, PureNtPath second)
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
        public static bool operator <=(PureNtPath first, PureNtPath second)
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
        public static bool operator >=(PureNtPath first, PureNtPath second)
        {
            return first == second || first > second;
        }

        /// <summary>
        /// Compare two <see cref="PureNtPath"/> for equality.
        /// Case insensitive.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(PureNtPath other)
        {
            return other != null && 
                NormCase().AsPosix().Equals(
                    other.NormCase().AsPosix());
        }

        /// <summary>
        /// Compare two <see cref="PureNtPath"/> for equality.
        /// Case insensitive.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(object other)
        {
            var obj = other as PureNtPath;
            return obj != null && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return NormCase().ToString().GetHashCode();
        }

        #endregion

        #region Operators

        /// <summary>
        /// Join two <see cref="PureNtPath"/>s.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static PureNtPath operator +(PureNtPath first, PureNtPath second)
        {
            return first.Join(second) as PureNtPath;
        }

        /// <summary>
        /// Join a <see cref="PureNtPath"/> with a string.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static PureNtPath operator +(PureNtPath first, string second)
        {
            return first.Join(second) as PureNtPath;
        }

        #endregion

        private bool? _cachedReserved;
        /// <inheritdoc/>
        public override bool IsReserved()
        {
            if(_cachedReserved.HasValue)
            {
                return _cachedReserved.Value;
            }
            foreach (var reservedPath in _reservedPaths)
            {
                if (!Filename.StartsWith(reservedPath)) continue;
                _cachedReserved = true;
                return true;
            }
            _cachedReserved = false;
            return false;
        }

        /// <inheritdoc/>
        public override bool Match(string pattern)
        {
            return PathUtils.Glob(
                pattern.ToLowerInvariant(), 
                NormCase().AsPosix(), 
                IsAbsolute());
        }

        /// <inheritdoc/>
        public override IPurePath NormCase()
        {
            return new PureNtPath(
                Drive.ToLowerInvariant(),
                Root, 
                Dirname.ToLowerInvariant(),
                Basename.ToLowerInvariant(),
                Extension.ToLowerInvariant());
        }

        /// <inheritdoc/>
        protected override IPurePath PurePathFactoryFromComponents(string drive, string root, string dirname, string basename, string extension)
        {
            return new PureNtPath(drive, root, dirname, basename, extension);
        }
    }
}
