using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using PathLib.Utils;

namespace PathLib
{
    // https://pathlib.readthedocs.org/en/latest/
    // https://docs.python.org/3/library/pathlib.html#module-pathlib
    // http://www.dotnetperls.com/path
    /// <summary>
    /// Base class containing common IPurePath code.
    /// </summary>
    public abstract class PurePath<TPath> : IPurePath<TPath>, IXmlSerializable
        where TPath : PurePath<TPath>
    {
        // Drive + Root + Dirname + Basename + Extension

        private const string UriPrefix = "file://";

        #region ctors

        /// <summary>
        /// Create a path in the current working directory.
        /// </summary>
        protected PurePath()
        {
            Drive = "";
            Root = "";
            Dirname = "";
            Basename = PathUtils.CurrentDirectoryIdentifier;
            Extension = "";
        }

        /// <summary>
        /// Create a path by joining the given path strings.
        /// </summary>
        /// <param name="parser">Parses parts out of a path.</param>
        /// <param name="paths">Paths to combine.</param>
        protected PurePath(IPathParser parser, params string[] paths)
        {
            string rawPath = null;
            if (paths.Length > 1)
            {
                var components = paths.Select(p => 
                    PurePathFactory(NormalizeSeparators(p)));
                var path = JoinInternal(components);
                rawPath = path.ToString();
                Assimilate(path);
            }
            else if (paths.Length == 1 && !String.IsNullOrEmpty(paths[0]))
            {
                rawPath = NormalizeSeparators(paths[0]);
                Drive = "";
                Root = "";
                Dirname = "";
                Basename = "";
                Extension = "";
            }
            else  // no paths
            {
                Drive = "";
                Root = "";
                Dirname = "";
                Basename = PathUtils.CurrentDirectoryIdentifier;
                Extension = "";
            }
            if (rawPath != null && 
                rawPath.StartsWith(NormalizeSeparators(UriPrefix)))
            {
                rawPath = rawPath.Substring(UriPrefix.Length);
            }
            Initialize(rawPath, parser);
        }

        /// <summary>
        /// Create a new PurePath from the specified components.
        /// </summary>
        /// <param name="drive"></param>
        /// <param name="root"></param>
        /// <param name="dirname"></param>
        /// <param name="basename"></param>
        /// <param name="extension"></param>
        protected PurePath(
            string drive, 
            string root, 
            string dirname, 
            string basename, 
            string extension)
        {
            Drive = drive ?? "";
            Root = root ?? "";
            Dirname = dirname ?? "";
            Basename = basename ?? "";
            Extension = extension ?? "";
        }

        /// <summary>
        /// Create a path by joining the given IPurePaths.
        /// </summary>
        /// <param name="paths">Paths to combine.</param>
        protected PurePath(params IPurePath[] paths)
        {
            Assimilate(JoinInternal(paths));
        }

        /// <summary>
        /// Replace the current path components with the given path's.
        /// </summary>
        /// <param name="path"></param>
        private void Assimilate(IPurePath path)
        {
            Drive = path.Drive ?? "";
            Root = path.Root ?? "";
            Dirname = path.Dirname ?? "";
            Basename = path.Basename ?? "";
            Extension = path.Extension ?? "";
        }

        private void Initialize(string rawPath, IPathParser parser)
        {
            if (rawPath == null)
            {
                return;
            }

            Drive = parser.ParseDrive(rawPath) ?? "";
            Root = parser.ParseRoot(rawPath) ?? "";

            if (Drive.Length + Root.Length >= rawPath.Length)
            {
                return;
            }

            rawPath = rawPath.Substring(Drive.Length + Root.Length);

            // Since the drive can contain invalid characters like '\\?\' or 
            // ':', we want to wait until after we parse the drive and root.
            char reservedCharacter;
            if (parser.ReservedCharactersInPath(rawPath, out reservedCharacter))
            {
                throw new InvalidPathException(rawPath, String.Format(
                    "Path contains reserved character '{0}'.", reservedCharacter));
            }

            // Remove trailing slash
            // This is what Python's pathlib does, but I don't think it's
            // necessarily required by spec
            if (rawPath.EndsWith(PathSeparator))
            {
                rawPath = rawPath.TrimEnd(PathSeparator.ToCharArray());
            }

            Dirname = parser.ParseDirname(rawPath) ?? "";
            rawPath = rawPath.Substring(Dirname.Length);

            Basename = parser.ParseBasename(rawPath) ?? "";
            rawPath = rawPath.Substring(Basename.Length);

            Extension = parser.ParseExtension(rawPath) ?? "";

            // If filename is just an extension, consider it a "hidden file"
            // where the leading dot is the filename, not the extension.
            if (Basename == String.Empty && Extension != String.Empty)
            {
                Basename = Extension;
                Extension = String.Empty;
            }

            Normalize();
        }

        #endregion

        #region Basic components of path

        /// <inheritdoc/>
        public string Drive { get; protected set; }

        /// <inheritdoc/>
        public string Root { get; protected set; }

        /// <inheritdoc/>
        public string Dirname { get; protected set; }

        /// <inheritdoc/>
        public string Basename { get; protected set; }

        /// <inheritdoc/>
        public string Extension { get; protected set; }

        #endregion
        
        /// <inheritdoc/>
        public string Anchor { get { return Drive + Root; } }

        /// <inheritdoc/>
        public string Directory 
        { 
            get 
            {
                if(!String.IsNullOrEmpty(Dirname))
                {
                    return Anchor + Dirname;
                }
                return String.Empty;
            } 
        }

        /// <inheritdoc/>
        public string Filename { get { return Basename + Extension; } }

        /// <inheritdoc/>
        public string BasenameWithoutExtensions
        {
            get
            {
                var parts = Filename.Split(PathUtils.ExtensionDelimiter);
                if (parts[0] == String.Empty && parts.Length > 1)
                {
                    // .dotfile is a filename, not an extension
                    return PathUtils.ExtensionDelimiter + parts[1];
                }
                return parts[0];
            }
        }

        /// <inheritdoc/>
        public string[] Extensions
        {
            get
            {
                var parts = Filename.Split(
                    new []{PathUtils.ExtensionDelimiter},
                    StringSplitOptions.RemoveEmptyEntries);
                var ret = new string[parts.Length - 1];
                for (var i = 0; i < ret.Length; i++)
                {
                    ret[i] = PathUtils.ExtensionDelimiter + parts[i + 1];
                }
                return ret;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> Parts
        {
            get 
            {
                if (_cachedParts == null)
                {
                    lock (_partsLock)
                    {
                        if (_cachedParts == null)
                        {
                            _cachedParts = BuildPartsArray();
                        }
                    }
                }
                return _cachedParts;
            }
        }
        private IEnumerable<string> _cachedParts;
        private readonly object _partsLock = new object();

        /// <inheritdoc/>
        private IEnumerable<string> BuildPartsArray()
        {
            if (Anchor != String.Empty)
            {
                yield return Anchor;
            }

            foreach (var part in Dirname.Split(
                new []{PathSeparator}, StringSplitOptions.RemoveEmptyEntries))
            {
                yield return part;
            }

            if (Filename != String.Empty)
            {
                yield return Filename;
            }
        }

        /// <inheritdoc/>
        public string ToPosix()
        {
            if (_cachedPosix == null)
            {
                lock (_toPosixLock)
                {
                    if (_cachedPosix == null)
                    {
                        _cachedPosix = ToString().Replace(@"\", "/");
                    }
                }
            }
            return _cachedPosix;
        }
        private string _cachedPosix;
        private readonly object _toPosixLock = new object();

        /// <inheritdoc/>
        public TPath Join(params string[] paths)
        {
            // TODO optimize for empty paths, return 'this'
            // TODO optimize for performance? currently ~400x slower than Path.Combine
            return JoinInternal(
                new[] { (TPath)this }
                    .Concat(paths.Select(PurePathFactory)));
        }

        public static PurePath<TPath> operator/ (PurePath<TPath> lvalue, PurePath<TPath> rvalue) 
        {
            return lvalue.JoinInternal(new[]{lvalue, rvalue});
        }

        public static PurePath<TPath> operator/ (PurePath<TPath> lvalue, string rvalue) 
        {
            return lvalue.JoinInternal(new[]{lvalue, lvalue.PurePathFactory(rvalue)});
        }

        IPurePath IPurePath.Join(params string[] paths)
        {
            return Join(paths);
        }

        /// <inheritdoc/>
        public TPath Join(params IPurePath[] paths)
        {
            return JoinInternal(new[] { this }.Concat(paths));
        }

        IPurePath IPurePath.Join(params IPurePath[] paths)
        {
            return Join(paths);
        }

        private TPath JoinInternal(IEnumerable<string> paths)
        {
            return JoinInternal(paths.Select(PurePathFactory));
        }
        
        private TPath JoinInternal(IEnumerable<TPath> paths)
        {
            return JoinInternal(paths.Select(p => (IPurePath)p));
        }

        private TPath JoinInternal(IEnumerable<IPurePath> paths)
        {
            var pathsList = new List<IPurePath>(paths);
            var path = PurePathFactoryFromComponents(
                PathUtils.Combine(pathsList, PathSeparator));
            if (path.Drive == String.Empty)
            {
                // Need to retain the last drive since the Combine chops off
                // the drive if an absolute path comes along later.
                var drive = pathsList
                    .Where(p => p.Drive != String.Empty)
                    .Select(p => p.Drive)
                    .LastOrDefault();
                if (drive != null)
                {
                    path = PurePathFactoryFromComponents(path, drive);
                }
            }

            return path;
        }

        /// <inheritdoc/>
        public bool TrySafeJoin(string relativePath, out TPath joined)
        {
            var toJoin = PurePathFactory(relativePath);
            string combined;
            if (!PathUtils.TrySafeCombine(this, toJoin, PathSeparator, out combined))
            {
                joined = null;
                return false;
            }

            joined = PurePathFactory(combined);
            return true;
        }

        /// <inheritdoc/>
        public bool TrySafeJoin(IPurePath relativePath, out TPath joined)
        {
            string combined;
            if (!PathUtils.TrySafeCombine(this, relativePath, PathSeparator, out combined))
            {
                joined = null;
                return false;
            }

            joined = PurePathFactory(combined);
            return true;
        }

        bool IPurePath.TrySafeJoin(string relativePath, out IPurePath joined)
        {
            TPath subPath;
            if(TrySafeJoin(relativePath, out subPath))
            {
                joined = subPath;
                return true;
            }
            joined = null;
            return false;
        }

        bool IPurePath.TrySafeJoin(IPurePath relativePath, out IPurePath joined)
        {
            TPath subPath;
            if (TrySafeJoin(relativePath, out subPath))
            {
                joined = subPath;
                return true;
            }
            joined = null;
            return false;
        }

        private string NormalizeSeparators(string path)
        {
            if (path is null) {
                throw new InvalidPathException("", "Path component was null");
            }
            foreach (var separator in PathUtils.PathSeparatorsForNormalization)
            {
                path = path.Replace(separator, PathSeparator);
            }
            return path;
        }

        /// <inheritdoc/>
        private void Normalize()
        {
            if (Dirname.Length <= 0) return;

            // Remove extra slashes (eg. foo///bar => foo/bar)
            // Leave initial double-slash (e.g. UNC paths)
            var newDirname = Regex.Replace(Dirname,
                                           Regex.Escape(PathSeparator) + "{2,}",
                                           PathSeparator);
            
            // Remove single dots (eg. foo/./bar => foo/bar)
            newDirname = Regex.Replace(newDirname, String.Format(
                "({0}{1}({0}|$))+", Regex.Escape(PathSeparator),
                Regex.Escape(PathUtils.CurrentDirectoryIdentifier)),
                                       @"$2");

            Dirname = newDirname;
        }

        /// <inheritdoc/>
        public TPath Parent()
        {
            return Parent(1);
        }

        IPurePath IPurePath.Parent()
        {
            return Parent();
        }

        /// <inheritdoc/>
        public TPath Parent(int nthParent)
        {
            return Parents().Skip(nthParent - 1).FirstOrDefault();
        }

        IPurePath IPurePath.Parent(int nthParent)
        {
            return Parent(nthParent);
        }

        /// <inheritdoc/>
        public IEnumerable<TPath> Parents()
        {
            var maxPathLength = Parts.Count() - 1;  // Don't return self as a parent
            for (var i = maxPathLength; i > 0; i--)
            {
                yield return PurePathFactory(
                    JoinInternal(
                        Parts.Take(i))
                    .ToString());
            }
        }

        IEnumerable<IPurePath> IPurePath.Parents()
        {
            return Parents().Select(p => (IPurePath)p);
        }

        /// <inheritdoc/>
        public Uri ToUri()
        {
            if (!IsAbsolute())
            {
                throw new InvalidOperationException(
                    "Cannot create a URI from a relative path.");
            }
            return new Uri(UriPrefix + ToPosix());
        }

        /// <inheritdoc/>
        public TPath RelativeTo(IPurePath parent)
        {
            if (!ComponentComparer.Equals(parent.Drive, Drive) || 
                !ComponentComparer.Equals(parent.Root, Root))
            {
                throw new ArgumentException(String.Format(
                    "'{0}' does not share the same root/drive as '{1}', " +
                    "thus cannot be relative.", this, parent));
            }

            var thisDirname = Dirname
                .Split(PathSeparator[0]).GetEnumerator();
            var parentRelative = parent.Relative().ToString();
            if (parentRelative == String.Empty)
            {
                return Relative();
            }

            var parentDirname = parentRelative.Split(PathSeparator[0]).GetEnumerator();
            while (parentDirname.MoveNext())
            {
                if (!thisDirname.MoveNext() ||
                    !ComponentComparer.Equals(parentDirname.Current, thisDirname.Current))
                {
                    throw new ArgumentException(String.Format(
                        "'{0}' does not start with '{1}'", this, parent));
                }
            }
            var builder = new StringBuilder();
            while (thisDirname.MoveNext())
            {
                if (builder.Length != 0)
                {
                    builder.Append(PathSeparator);
                }
                builder.Append(thisDirname.Current);
            }
            return PurePathFactoryFromComponents(
                null, null, null, builder.ToString(), Basename, Extension);
        }

        IPurePath IPurePath.RelativeTo(IPurePath parent)
        {
            return RelativeTo(parent);
        }

        /// <inheritdoc/>
        public TPath WithDirname(IPurePath newDirname)
        {
            // Format separators, remove extra separators, and
            // exclude Drive (if present)
            var formatted = newDirname.GetComponents(
                    PathComponent.Dirname | PathComponent.Filename);
            if (IsAbsolute() || !newDirname.IsAbsolute())
            {
                return PurePathFactoryFromComponents(this,
                    dirname: formatted);
            }
            return PurePathFactoryFromComponents(this,
                newDirname.Drive,
                newDirname.Root,
                formatted);
        }

        /// <inheritdoc/>
        public TPath WithDirname(string newDirname)
        {
            return WithDirname(PurePathFactory(newDirname));
        }

        IPurePath IPurePath.WithDirname(string newDirname)
        {
            return String.IsNullOrEmpty(newDirname)
                ? this 
                : WithDirname(PurePathFactory(newDirname));
        }

        IPurePath IPurePath.WithDirname(IPurePath newDirname)
        {
            return WithDirname(newDirname);
        }

        /// <inheritdoc/>
        public TPath WithExtension(string newExtension)
        {
            var fname = PurePathFactory(newExtension);
            // Allows setting the extension with or without the '.'
            if (fname.HasComponents(PathComponent.All & 
                ~(PathComponent.Basename | PathComponent.Extension)))
            {
                throw new InvalidPathException(newExtension,
                    "Path must contain only extension.");
            }
            if (fname.HasComponents(PathComponent.Extension))
            {
                // Multiple extensions... place the extras on the basename
                return PurePathFactoryFromComponents(this,
                    basename: Basename + PrependWithDot(fname.Basename),
                    extension: PrependWithDot(fname.Extension));
            }

            return PurePathFactoryFromComponents(this,
                extension: PrependWithDot(fname.Basename));
        }

        private static string PrependWithDot(string extension)
        {
            if (extension.StartsWith("" + PathUtils.ExtensionDelimiter))
            {
                return extension;
            }
            return PathUtils.ExtensionDelimiter + extension;
        }

        IPurePath IPurePath.WithExtension(string newExtension)
        {
            return WithExtension(newExtension);
        }

        /// <inheritdoc/>
        public TPath WithFilename(string newFilename)
        {
            if (String.IsNullOrEmpty(newFilename))
            {
                return PurePathFactoryFromComponents(
                    this, basename: "", extension: "");
            }

            var fname = PurePathFactory(newFilename);
            if (fname.HasComponents(PathComponent.All & ~PathComponent.Filename))
            {
                throw new ArgumentException(String.Format(
                    "New filename '{0}' must contain only basename and/or extension.", newFilename),
                    "newFilename");
            }
            return PurePathFactoryFromComponents(this,
                basename: fname.Basename, extension: fname.Extension);
        }

        IPurePath IPurePath.WithFilename(string newFilename)
        {
            return WithFilename(newFilename);
        }

        /// <inheritdoc/>
        public bool HasComponents(PathComponent components)
        {
            if ((components & PathComponent.Drive) == PathComponent.Drive
                && Drive != String.Empty)
            {
                return true;
            }
            if ((components & PathComponent.Root) == PathComponent.Root
                && Root != String.Empty)
            {
                return true;
            }
            if ((components & PathComponent.Dirname) == PathComponent.Dirname
                && Dirname != String.Empty)
            {
                return true;
            }
            if ((components & PathComponent.Basename) == PathComponent.Basename
                && Basename != String.Empty)
            {
                return true;
            }
            if ((components & PathComponent.Extension) == PathComponent.Extension
                && Extension != String.Empty)
            {
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public string GetComponents(PathComponent components)
        {
            var builder = new StringBuilder();
            
            if ((components & PathComponent.Drive) == PathComponent.Drive)
            {
                builder.Append(Drive);
            }
            if ((components & PathComponent.Root) == PathComponent.Root)
            {
                builder.Append(Root);
            }
            string path = null;
            if ((components & PathComponent.Dirname) == PathComponent.Dirname)
            {
                path = Dirname;
            }
            if ((components & PathComponent.Basename) == PathComponent.Basename
                && Basename != String.Empty)
            {
                path = !String.IsNullOrEmpty(path) 
                    ? PathUtils.Combine(path, Basename, PathSeparator) 
                    : Basename;
            }
            if ((components & PathComponent.Extension) == PathComponent.Extension)
            {
                path += Extension;
            }
            if (path != null)
            {
                builder.Append(path);
            }
            return builder.ToString();
        }

        private string _cachedToString;
        /// <inheritdoc/>
        public override string ToString()
        {
            return _cachedToString ??= GetComponents(PathComponent.All);
        }

        /// <inheritdoc/>
        protected abstract string PathSeparator { get; }

        /// <summary>
        /// Allows comparisons between components to be made regardless of
        /// current filesystem rules.
        /// </summary>
        protected abstract StringComparer ComponentComparer { get; }

        /// <inheritdoc/>
        public bool IsAbsolute()
        {
            // Does not use anchor because "C:path\foo.txt"
            // counts as a relative path in a different drive.
            return !String.IsNullOrEmpty(Root);
        }

        #region Equality Members

        /*
        public static bool operator ==(PurePath<TPath> first, PurePath<TPath> second)
        {
            return ReferenceEquals(first, null) ?
                ReferenceEquals(second, null) :
                first.Equals(second);
        }


        public static bool operator !=(PurePath<TPath> first, PurePath<TPath> second)
        {
            return !(first == second);
        }
        */

  
        #endregion
        

        /// <inheritdoc/>
        public abstract bool IsReserved();

        // Matching is case-insensitive on NT machines.
        // http://stackoverflow.com/questions/6907720/need-to-perform-wildcard-etc-search-on-a-string-using-regex/16488364#16488364
        /// <inheritdoc/>
        public abstract bool Match(string pattern);

        /// <inheritdoc/>
        public TPath NormCase()
        {
            return NormCase(CultureInfo.CurrentCulture);
        }

        /// <inheritdoc/>
        public abstract TPath NormCase(CultureInfo currentCulture);

        IPurePath IPurePath.NormCase(CultureInfo currentCulture)
        {
            return NormCase(currentCulture);
        }

        IPurePath IPurePath.NormCase()
        {
            return NormCase(CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Create an instance of your own IPurePath implementation
        /// when given the path to use.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected abstract TPath PurePathFactory(string path);

        /// <inheritdoc/>
        protected TPath PurePathFactoryFromComponents(
            IPurePath original, 
            string drive = null, 
            string root = null, 
            string dirname = null, 
            string basename = null, 
            string extension = null)
        {
            return PurePathFactoryFromComponents(
                drive ?? (original != null ? original.Drive : ""),
                root ?? (original != null ? original.Root : ""),
                dirname ?? (original != null ? original.Dirname : ""),
                basename ?? (original != null ? original.Basename : ""),
                extension ?? (original != null ? original.Extension : ""));
        }

        /// <inheritdoc/>
        protected abstract TPath PurePathFactoryFromComponents(
            string drive,
            string root,
            string dirname,
            string basename,
            string extension);

        /// <inheritdoc/>
        public TPath Relative()
        {
            return PurePathFactoryFromComponents(
                this, String.Empty, String.Empty);
        }

        IPurePath IPurePath.Relative()
        {
            return Relative();
        }

        #region Xml Serialization

        /// <inheritdoc/>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <inheritdoc/>
        public virtual void ReadXml(System.Xml.XmlReader reader)
        {
            var path = PurePathFactory(reader.ReadString());
            reader.ReadEndElement();

            Assimilate(path);
        }

        /// <inheritdoc/>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteString(ToString());
        }

        #endregion
    }
}
