using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace PathLib
{
    // https://pathlib.readthedocs.org/en/latest/
    // http://www.dotnetperls.com/path
    /// <summary>
    /// Base class containing common IPurePath code.
    /// </summary>
    public abstract class PurePath : IPurePath, IXmlSerializable
    {
        /// <summary>
        /// A string representing the operating system's "current directory"
        /// identifier.
        /// </summary>
        public const string CurrentDirectoryIdentifier = ".";

        /// <summary>
        /// A string representing the operating system's "parent directory"
        /// identifier.
        /// </summary>
        public const string ParentDirectoryIdentifier = "..";

        /// <summary>
        /// A char representing the character delimiting extensions
        /// from filenames.
        /// </summary>
        public const char ExtensionDelimiter = '.';

        /// <summary>
        /// A string representing the character delimiting drives from the
        /// remaining path.
        /// </summary>
        public const char DriveDelimiter = ':';

        private static readonly string[] PathSeparatorsForNormalization = {"/", @"\"};

        // Drive + Root + Dirname + Basename + Extension
        /// <summary>
        /// The raw, unmodified path passed in to the constructor (if
        /// available).
        /// </summary>
        protected readonly string RawPath;

        #region Factories

        /// <summary>
        /// Factory method do create a new PurePath instance based upon
        /// the current operating system.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static PurePath Create(params string[] paths)
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
            RawPath = CurrentDirectoryIdentifier;

            Drive = "";
            Root = "";
            Dirname = CurrentDirectoryIdentifier;
            Basename = "";
            Extension = "";
        }

        /// <summary>
        /// Create a path by joining the given path strings.
        /// </summary>
        /// <param name="paths">Paths to combine.</param>
        protected PurePath(params string[] paths)
        {
            if (paths.Length > 1)
            {
                var components = LinqBridge.Select(paths, p => 
                        PurePathFactory(NormalizeSeparators(p)));
                var path = JoinInternal(components);
                RawPath = path.ToString();
                Assimilate(path);
            }
            else if(paths.Length == 1)
            {
                RawPath = NormalizeSeparators(paths[0]);
                Drive = "";
                Root = "";
                Dirname = "";
                Basename = "";
                Extension = "";
            }
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
        public string Filename { get { return Basename + Extension; } }

        /// <inheritdoc/>
        public string[] Extensions
        {
            get
            {
                var parts = Filename.Split(ExtensionDelimiter);
                var ret = new string[parts.Length - 1];
                for (var i = 0; i < ret.Length; i++)
                {
                    ret[i] = ExtensionDelimiter + parts[i + 1];
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
        public IPurePath Join(params string[] paths)
        {
            return JoinInternal(
                LinqBridge.Concat(
                    new[] { this },
                    LinqBridge.Select(paths, PurePathFactory)));
        }

        /// <inheritdoc/>
        public IPurePath Join(params IPurePath[] paths)
        {
            return JoinInternal(LinqBridge.Concat(new[] { this }, paths));
        }

        private IPurePath JoinInternal(IEnumerable<string> paths)
        {
            return JoinInternal(LinqBridge.Select(paths, PurePathFactory));
        }
        
        private IPurePath JoinInternal(IEnumerable<IPurePath> paths)
        {
            var pathsList = new List<IPurePath>(paths);
            var path = PathUtils.Combine(pathsList, PathSeparator);

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

        private string NormalizeSeparators(string path)
        {
            foreach (var separator in PathSeparatorsForNormalization)
            {
                path = path.Replace(separator, PathSeparator);
            }
            return path;
        }

        /// <inheritdoc/>
        protected void Normalize(IPurePath path)
        {
            if (path.Dirname.Length <= 0) return;

            // Remove extra slashes (eg. foo///bar => foo/bar)
            // Leave initial double-slash (e.g. UNC paths)
            var newDirname = Regex.Replace(path.Dirname,
                                           Regex.Escape(PathSeparator) + "{2,}",
                                           PathSeparator);
            
            // Remove single dots (eg. foo/./bar => foo/bar)
            newDirname = Regex.Replace(newDirname, String.Format(
                "({0}{1}({0}|$))+", Regex.Escape(PathSeparator),
                Regex.Escape(CurrentDirectoryIdentifier)),
                                       @"$2");

            if (newDirname != path.Dirname)
            {
                Assimilate(PurePathFactoryFromComponents(path, dirname: newDirname));
            }
        }

        /// <inheritdoc/>
        public IPurePath Parent()
        {
            return Parent(1);
        }

        /// <inheritdoc/>
        public IPurePath Parent(int nthParent)
        {
            return LinqBridge.FirstOrDefault(
                LinqBridge.Skip(Parents(), nthParent - 1));
        }

        /// <inheritdoc/>
        public IEnumerable<IPurePath> Parents()
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
        public IPurePath RelativeTo(IPurePath parent)
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

        /// <inheritdoc/>
        public IPurePath WithDirname(string newDirname)
        {
            return PurePathFactoryFromComponents(this, dirname: newDirname);
        }

        /// <inheritdoc/>
        public IPurePath WithExtension(string newExtension)
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
                    ? ExtensionDelimiter + fname.Basename 
                    : fname.Extension);
        }

        /// <inheritdoc/>
        public IPurePath WithFilename(string newFilename)
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
        public abstract IPurePath NormCase();

        /// <summary>
        /// Create an instance of your own IPurePath implementation
        /// when given the path to use.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected abstract IPurePath PurePathFactory(string path);

        /// <inheritdoc/>
        protected IPurePath PurePathFactoryFromComponents(
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
        protected abstract IPurePath PurePathFactoryFromComponents(
            string drive,
            string root,
            string dirname,
            string basename,
            string extension);

        /// <inheritdoc/>
        public IPurePath Relative()
        {
            return PurePathFactoryFromComponents(
                this, String.Empty, String.Empty);
        }

        #region Xml Serialization

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public virtual void ReadXml(System.Xml.XmlReader reader)
        {
            var path = PurePathFactory(reader.ReadString());
            reader.ReadEndElement();

            Assimilate(path);
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteString(ToString());
            writer.WriteEndElement();
        }

        #endregion
    }
}
