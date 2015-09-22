using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using PathLib.Utils;

namespace PathLib
{
    // TODO: verify against https://blogs.msdn.microsoft.com/jeremykuhne/2016/04/21/path-normalization/
    
    /// <summary>
    /// Represents an NT path. Uses the backslash for a separator and
    /// treats paths as case insensitive.
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa365247(v=vs.85).aspx
    /// </summary>
    [TypeConverter(typeof(PureWindowsPathConverter))]
    public sealed class PureWindowsPath : PurePath<PureWindowsPath>, IEquatable<PureWindowsPath>
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
        public PureWindowsPath()
        {
        }

        /// <summary>
        /// Create an NT path by joining the given path strings.
        /// Uses the backslash for a separator and treats paths as
        /// case insensitive.
        /// </summary>
        /// <param name="paths">Paths to combine.</param>
        public PureWindowsPath(params string[] paths)
            : base(new WindowsPathParser(), paths)
        {
        }

        /// <summary>
        /// Create an NT path by joining the given IPurePaths.
        /// Uses the backslash for a separator and treats paths as
        /// case insensitive.
        /// </summary>
        /// <param name="paths">Paths to combine.</param>
        public PureWindowsPath(params IPurePath[] paths)
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
        private PureWindowsPath(string drive, string root, string dirname, string basename, string extension)
            : base(drive, root, dirname, basename, extension)
        {
        }

        #endregion

        private class WindowsPathParser : IPathParser
        {
            private readonly char[] _reservedCharacters =
            {
                '<', '>', ':', '|', '"', '?', '*', '\u0000'
                //'/', '\\'  // Technically reserved, but parsed here as path characters
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

        /// <summary>
        /// Attempt to parse a given string as a PureWindowsPath.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParse(string path, out PureWindowsPath result)
        {
            try
            {
                result = new PureWindowsPath(path);
                return true;
            }
            catch(InvalidPathException)
            {
                result = null;
                return false;
            }
        }

        /// <inheritdoc/>
        protected override PureWindowsPath PurePathFactory(string path)
        {
            return new PureWindowsPath(path);
        }

        /// <inheritdoc/>
        protected override string PathSeparator
        {
            get { return @"\"; }
        }

        /// <inheritdoc/>
        protected override StringComparer ComponentComparer
        {
            get { return StringComparer.CurrentCultureIgnoreCase; }  // TODO allow custom culture
        }

        #region Parsing Initializers

        #endregion

        #region Equality Members

        /// <summary>
        /// Compare two <see cref="PureWindowsPath"/> for equality.
        /// Case insensitive.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator ==(PureWindowsPath first, PureWindowsPath second)
        {
            return ReferenceEquals(first, null) ?
                ReferenceEquals(second, null) :
                first.Equals(second);
        }

        /// <summary>
        /// Compare two <see cref="PureWindowsPath"/> for inequality.
        /// Case insensitive.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator !=(PureWindowsPath first, PureWindowsPath second)
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
        public static bool operator <(PureWindowsPath first, PureWindowsPath second)
        {
            if (ReferenceEquals(first, null) || ReferenceEquals(second, null))
            {
                return false;
            }

            // Resolve symlinks before comparing
            var parent = new List<string>(first.NormCase().Parts);
            var child = new List<string>(second.NormCase().Parts);

            // Parent must be shorter than child
            if (parent.Count() >= child.Count())
            {
                return false;
            }
            foreach (var parts in parent.Zip(child, (p, c) => new [] {p, c}))
            {
                if (!String.Equals(parts[0], parts[1], StringComparison.InvariantCultureIgnoreCase))
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
        public static bool operator >(PureWindowsPath first, PureWindowsPath second)
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
        public static bool operator <=(PureWindowsPath first, PureWindowsPath second)
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
        public static bool operator >=(PureWindowsPath first, PureWindowsPath second)
        {
            return first == second || first > second;
        }

        /// <summary>
        /// Compare two <see cref="PureWindowsPath"/> for equality.
        /// Case insensitive.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(PureWindowsPath other)
        {
            return !ReferenceEquals(other, null) && 
                NormCase().ToString().Equals(
                    other.NormCase().ToString());
        }

        /// <summary>
        /// Compare two <see cref="PureWindowsPath"/> for equality.
        /// Case insensitive.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(object other)
        {
            var obj = other as PureWindowsPath;
            return !ReferenceEquals(obj, null) && Equals(obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return NormCase().ToString().GetHashCode();
        }

        #endregion

        #region Operators

        /// <summary>
        /// Join two <see cref="PureWindowsPath"/>s.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static PureWindowsPath operator +(PureWindowsPath first, PureWindowsPath second)
        {
            return first.Join(second);
        }

        /// <summary>
        /// Join a <see cref="PureWindowsPath"/> with a string.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static PureWindowsPath operator +(PureWindowsPath first, string second)
        {
            return first.Join(second);
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
                pattern, 
                ToString(), 
                IsAbsolute(), true);
        }

        /// <inheritdoc/>
        public override PureWindowsPath NormCase(CultureInfo currentCulture)
        {
            return new PureWindowsPath(
                Drive.ToLower(currentCulture),
                Root, 
                Dirname.ToLower(currentCulture),
                Basename.ToLower(currentCulture),
                Extension.ToLower(currentCulture));
        }

        /// <inheritdoc/>
        protected override PureWindowsPath PurePathFactoryFromComponents(string drive, string root, string dirname, string basename, string extension)
        {
            return new PureWindowsPath(drive, root, dirname, basename, extension);
        }
    }
}
