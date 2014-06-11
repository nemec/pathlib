using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PathLib
{
    /// <summary>
    /// Represents an NT path. Uses the backslash for a separator and
    /// treats paths as case insensitive.
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa365247(v=vs.85).aspx
    /// </summary>
    [TypeConverter(typeof(Converters.PureNtPathConverter))]
    public class PureNtPath : PurePath<PureNtPath>, IEquatable<PureNtPath>
    {
        private readonly string[] _reservedPaths =
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
        }

        /// <summary>
        /// Create an NT path by joining the given path strings.
        /// Uses the backslash for a separator and treats paths as
        /// case insensitive.
        /// </summary>
        /// <param name="paths">Paths to combine.</param>
        public PureNtPath(params string[] paths)
            : base(new NtPathParser(), paths)
        {
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
        }

        #endregion

        private class NtPathParser : IPathParser
        {
            private readonly char[] _reservedCharacters =
            {
                '<', '>', '|'
            };

            private const string PathSeparator = @"\";

            public string ParseDrive(string remainingPath)
            {
                return !String.IsNullOrEmpty(remainingPath)
                    ? PathUtils.GetPathRoot(remainingPath, PathSeparator).TrimEnd(PathSeparator[0])
                    : null;
            }

            public string ParseRoot(string remainingPath)
            {
                if (String.IsNullOrEmpty(remainingPath))
                {
                    return null;
                }

                var root = PathUtils.GetPathRoot(remainingPath, PathSeparator);
                if (root.StartsWith(PathSeparator))
                {
                    return PathSeparator;
                }
                return root.EndsWith(PathSeparator)
                           ? PathSeparator
                           : null;
            }

            public string ParseDirname(string remainingPath)
            {
                // Hardcode special dirs
                if (remainingPath == "." || remainingPath == "..")
                {
                    return remainingPath;
                }
                return PathUtils.GetDirectoryName(remainingPath, PathSeparator);
            }

            public string ParseBasename(string remainingPath)
            {
                return !String.IsNullOrEmpty(remainingPath)
                    ? remainingPath != PathUtils.CurrentDirectoryIdentifier
                        ? PathUtils.GetFileNameWithoutExtension(remainingPath, PathSeparator)
                            : PathUtils.CurrentDirectoryIdentifier
                    : null;
            }

            public string ParseExtension(string remainingPath)
            {
                return !String.IsNullOrEmpty(remainingPath)
                    ? PathUtils.GetExtension(remainingPath, PathSeparator)
                    : null;
            }

            public bool ReservedCharactersInPath(string path, out char reservedCharacter)
            {
                foreach (var ch in _reservedCharacters)
                {
                    if (path.IndexOf(ch) >= 0)
                    {
                        reservedCharacter = ch;
                        return true;
                    }
                }
                reservedCharacter = default (char);
                return false;
            }
        }

        /// <inheritdoc/>
		public static bool TryParse(string path, out PureNtPath result)
		{
			try
			{
				result = new PureNtPath(path);
				return true;
			}
			catch(InvalidPathException)
			{
				result = null;
				return false;
			}
		}

        /// <inheritdoc/>
        protected override PureNtPath PurePathFactory(string path)
        {
            return new PureNtPath(path);
        }

        /// <inheritdoc/>
        protected override string PathSeparator
        {
            get { return @"\"; }
        }

        #region Parsing Initializers

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
            var parent = new List<string>(first.NormCase().Parts);
            var child = new List<string>(second.NormCase().Parts);

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
            return obj != null && Equals(obj);
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
        public override PureNtPath NormCase()
        {
            return new PureNtPath(
                Drive.ToLowerInvariant(),
                Root, 
                Dirname.ToLowerInvariant(),
                Basename.ToLowerInvariant(),
                Extension.ToLowerInvariant());
        }

        /// <inheritdoc/>
        protected override PureNtPath PurePathFactoryFromComponents(string drive, string root, string dirname, string basename, string extension)
        {
            return new PureNtPath(drive, root, dirname, basename, extension);
        }
    }
}
