using System;
using System.ComponentModel;
using System.Globalization;
using PathLib.Utils;

namespace PathLib
{
    /// <summary>
    /// Represents a POSIX path. Uses the slash as a directory separator
    /// and treats paths as case sensitive.
    /// </summary>
    [TypeConverter(typeof(PurePosixPathConverter))]
    public sealed class PurePosixPath : PurePath<PurePosixPath>, IEquatable<PurePosixPath>
    {
        #region ctors

        /// <summary>
        /// Create a path in the current working directory.
        /// </summary>
        public PurePosixPath()
        {
        }

        /// <summary>
        /// Create a path by joining the given path strings.
        /// </summary>
        /// <param name="paths">Paths to combine.</param>
        public PurePosixPath(params string[] paths)
            : base(new PosixParser(), paths)
        {
        }

        /// <summary>
        /// Create a path by joinin the given IPurePaths.
        /// </summary>
        /// <param name="paths">Paths to combine.</param>
        public PurePosixPath(params IPurePath[] paths)
            : base(paths)
        {
        }

        /// <inheritdoc/>
        private PurePosixPath(string drive, string root, string dirname, string basename, string extension)
            : base(drive, root, dirname, basename, extension)
        {
        }

        #endregion

        private class PosixParser : IPathParser
        {
            private const string PathSeparator = "/";

            public string ParseDrive(string remainingPath)
            {
                return null;
            }

            public string ParseRoot(string remainingPath)
            {
                // Special case in POSIX pathname resolution
                // http://pubs.opengroup.org/onlinepubs/009695399/basedefs/xbd_chap04.html#tag_04_11
                if (remainingPath.StartsWith(PathSeparator + PathSeparator) &&
                    (remainingPath.Length <= 2 || (remainingPath[2]) != PathSeparator[0]))
                {
                    return PathSeparator + PathSeparator;
                }
                return remainingPath.StartsWith(PathSeparator) 
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
                reservedCharacter = default(char);
                if (path.Contains("\u0000"))
                {
                    reservedCharacter = '\u0000';
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Attempt to parse a given string as a PureWindowsPath.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParse(string path, out PurePosixPath result)
        {
            try
            {
                result = new PurePosixPath(path);
                return true;
            }
            catch (InvalidPathException)
            {
                result = null;
                return false;
            }
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
            if (ReferenceEquals(first, null) || ReferenceEquals(second, null))
            {
                return false;
            }

            if (LinqBridge.Count(first.Parts) >= LinqBridge.Count(second.Parts))
            {
                return false;
            }

            foreach (var parts in LinqBridge.Zip(
                first.Parts, second.Parts, (p, c) => new[]{p, c}))
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
            return other != null && ToPosix().Equals(other.ToPosix());
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
            return ToPosix().GetHashCode();
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
            return first.Join(second);
        }

        /// <summary>
        /// Join a <see cref="PurePosixPath"/> with a string.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static PurePosixPath operator +(PurePosixPath first, string second)
        {
            return first.Join(second);
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
            return PathUtils.Glob(pattern, ToString(), IsAbsolute());
        }

        /// <inheritdoc/>
        public override PurePosixPath NormCase(CultureInfo currentCulture)
        {
            return this;
        }
    }
}
