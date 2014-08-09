using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

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

        #region Factories

        /// <summary>
        /// Factory method do create a new PurePath instance based upon
        /// the current operating system.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static IPurePath Create(params string[] paths)
        {
            var p = Environment.OSVersion.Platform;
            // http://mono.wikia.com/wiki/Detecting_the_execution_platform
            // 128 required for early versions of Mono
            if (p == PlatformID.Unix || p == PlatformID.MacOSX || (int)p == 128)
            {
                return new PurePosixPath(paths);
            }
            return new PureNtPath(paths);
        }

        #endregion

        #region ctors

        /// <summary>
        /// Create a path in the current working directory.
        /// </summary>
        protected PurePath()
        {
            Drive = "";
            Root = "";
            Dirname = PathUtils.CurrentDirectoryIdentifier;
            Basename = "";
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
                var components = LinqBridge.Select(paths, p => 
                        PurePathFactory(NormalizeSeparators(p)));
                var path = JoinInternal(components);
                rawPath = path.ToString();
                Assimilate(path);
            }
            else if (paths.Length == 1)
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
                Dirname = PathUtils.CurrentDirectoryIdentifier;
                Basename = "";
                Extension = "";
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
            char reservedCharacter;
            if (parser.ReservedCharactersInPath(rawPath, out reservedCharacter))
            {
                throw new InvalidPathException(rawPath, String.Format(
                    "Path contains reserved character '{0}'.", reservedCharacter));
            }

            Drive = parser.ParseDrive(rawPath) ?? "";
            Root = parser.ParseRoot(rawPath) ?? "";

            if (Drive.Length + Root.Length >= rawPath.Length)
            {
                return;
            }

            rawPath = rawPath.Substring(Drive.Length + Root.Length);

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
        public string[] Extensions
        {
            get
            {
                var parts = Filename.Split(PathUtils.ExtensionDelimiter);
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
                return _cachedParts ?? (_cachedParts = BuildPartsArray());
            }
        }
        private IEnumerable<string> _cachedParts;

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

            if (Filename != string.Empty)
            {
                yield return Filename;
            }
        }

        private string _cachedPosix;
        /// <inheritdoc/>
        public string AsPosix()
        {
            return _cachedPosix ?? (_cachedPosix = ToString().Replace(@"\", "/"));
        }

        /// <inheritdoc/>
        public IPurePath<TPath> Join(params string[] paths)
        {
            return JoinInternal(
                LinqBridge.Concat(
                    new[] { (TPath)this },
                    LinqBridge.Select(paths, PurePathFactory)));
        }

		IPurePath IPurePath.Join(params string[] paths)
		{
			return Join(paths);
		}

        /// <inheritdoc/>
        public IPurePath<TPath> Join(params IPurePath[] paths)
        {
            return JoinInternal(LinqBridge.Concat(new[] { this }, paths));
        }

        IPurePath IPurePath.Join(params IPurePath[] paths)
        {
            return Join(paths);
        }

        private TPath JoinInternal(IEnumerable<string> paths)
        {
            return JoinInternal(LinqBridge.Select(paths, PurePathFactory));
        }
        
        private TPath JoinInternal(IEnumerable<TPath> paths)
        {
            return JoinInternal(LinqBridge.Select(paths, p => (IPurePath)p));
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
                var drive = LinqBridge.LastOrDefault(
                    LinqBridge.Select(
                        LinqBridge.Where(pathsList, p => p.Drive != String.Empty),
                        p => p.Drive));
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
            return LinqBridge.FirstOrDefault(
                LinqBridge.Skip(Parents(), nthParent - 1));
        }

        IPurePath IPurePath.Parent(int nthParent)
        {
            return Parent(nthParent);
        }

        /// <inheritdoc/>
        public IEnumerable<TPath> Parents()
        {
            var maxPathLength = LinqBridge.Count(Parts) - 1;  // Don't return self as a parent
            for (var i = maxPathLength; i > 0; i--)
            {
                yield return PurePathFactory(
                    JoinInternal(
                        LinqBridge.Take(Parts, i))
                    .ToString());
            }
        }

        IEnumerable<IPurePath> IPurePath.Parents()
        {
            return LinqBridge.Select(Parents(), p => (IPurePath)p);
        }

        /// <inheritdoc/>
        public Uri AsUri()
        {
            if (!IsAbsolute())
            {
                throw new InvalidOperationException(
                    "Cannot create a URI from a relative path.");
            }
            return new Uri("file://" + AsPosix());
        }

        /// <inheritdoc/>
        public TPath RelativeTo(IPurePath parent)
        {
            if (parent.Drive != Drive || parent.Root != Root)
            {
                throw new ArgumentException(String.Format(
                    "'{0}' does not start with '{1}'", this, parent));
            }

            var thisDirname = Dirname
                .Split(PathSeparator[0]).GetEnumerator();
            var parentDirname = parent.Relative().ToString()
                .Split(PathSeparator[0]).GetEnumerator();
            while (parentDirname.MoveNext())
            {
                if (!thisDirname.MoveNext() ||
                    !Equals(parentDirname.Current, thisDirname.Current))
                {
                    throw new ArgumentException(String.Format(
                        "'{0}' does not start with '{1}'", this, parent));
                }
            }
            var builder = new StringBuilder();
            while (thisDirname.MoveNext())
            {
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
        public TPath WithDirname(string newDirname)
        {
            return PurePathFactoryFromComponents(this, dirname: newDirname);
        }

        IPurePath IPurePath.WithDirname(string newDirname)
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
            return PurePathFactoryFromComponents(this,
                extension: fname.HasComponents(PathComponent.Basename)
                    ? fname.Basename.StartsWith(""+PathUtils.ExtensionDelimiter)
                        ? fname.Basename
                        : PathUtils.ExtensionDelimiter + fname.Basename 
                    : fname.Extension);
        }

        IPurePath IPurePath.WithExtension(string newExtension)
        {
            return WithExtension(newExtension);
        }

        /// <inheritdoc/>
        public TPath WithFilename(string newFilename)
        {
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return GetComponents(PathComponent.All);
        }

        /// <inheritdoc/>
        protected abstract string PathSeparator { get; }

        /// <inheritdoc/>
        public bool IsAbsolute()
        {
            // Does not use anchor because "C:path\foo.txt"
            // counts as a relative path in a different drive.
            return !String.IsNullOrEmpty(Root);
        }

        /// <inheritdoc/>
        public abstract bool IsReserved();

        // Matching is case-insensitive on NT machines.
        // http://stackoverflow.com/questions/6907720/need-to-perform-wildcard-etc-search-on-a-string-using-regex/16488364#16488364
        /// <inheritdoc/>
        public abstract bool Match(string pattern);

        /// <inheritdoc/>
        public abstract TPath NormCase();

        IPurePath IPurePath.NormCase()
        {
            return NormCase();
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
            writer.WriteEndElement();
        }

        #endregion
    }
}
