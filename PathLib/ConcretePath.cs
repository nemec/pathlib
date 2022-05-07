using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using PathLib.Utils;

namespace PathLib
{
    /// <summary>
    /// Base class for common methods in concrete paths.
    /// </summary>
    public abstract class ConcretePath<TPath, TPurePath> : IPath<TPath, TPurePath>
        where TPath : ConcretePath<TPath, TPurePath>, IPath
        where TPurePath : IPurePath<TPurePath>
    {
        /// <inheritdoc/>
        public readonly TPurePath PurePath;

        /// <inheritdoc/>
        protected ConcretePath(TPurePath purePath)
        {
            PurePath = purePath;
        }

        /// <inheritdoc/>
        public FileSize Size
        {
            get
            {
                var data = Restat();
                return new FileSize(data.Size);
            }
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
        public FileInfo FileInfo
        {
            get
            {
                if (_fileInfoCache != null)
                {
                    return _fileInfoCache;
                }
                if (Exists() && !IsFile())
                {
                    return null;
                }
                return (_fileInfoCache = new FileInfo(ToString()));
            }
        }
        private FileInfo _fileInfoCache;

        /// <inheritdoc/>
        public DirectoryInfo DirectoryInfo
        {
            get
            {
                if (_directoryInfoCache != null)
                {
                    return _directoryInfoCache;
                }
                if (Exists() && !IsDir())
                {
                    return null;
                }
                return (_directoryInfoCache = new DirectoryInfo(ToString()));
            }
        }

        private DirectoryInfo _directoryInfoCache;

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
        public bool IsFile()
        {
            return File.Exists(PurePath.ToPosix());
        }

        /// <inheritdoc/>
        public bool IsDir()
        {
            return System.IO.Directory.Exists(PurePath.ToPosix());
        }

        /// <inheritdoc/>
        public IEnumerable<TPath> ListDir()
        {
            return ListDir("*", SearchOption.TopDirectoryOnly);
        }

        /// <inheritdoc/>
        IEnumerable<IPath> IPath.ListDir()
        {
            return ListDir().Select(p => (IPath)p);
        }

        /// <inheritdoc/>
        public IEnumerable<TPath> ListDir(string pattern)
        {
            return ListDir(pattern, SearchOption.TopDirectoryOnly);
        }

        IEnumerable<IPath> IPath.ListDir(string pattern)
        {
            return ListDir(pattern).Select(p => (IPath)p);
        }

        /// <inheritdoc/>
        public IEnumerable<TPath> ListDir(SearchOption scope)
        {
            return ListDir("*", scope);
        }

        IEnumerable<IPath> IPath.ListDir(SearchOption scope)
        {
            return ListDir(scope).Select(p => (IPath)p);
        }

        /// <inheritdoc/>
        public IEnumerable<TPath> ListDir(string pattern, SearchOption scope)
        {
            if (!IsDir())
            {
                throw new ArgumentException("Glob may only be called on directories.");
            }
            foreach (var dir in DirectoryInfo.GetDirectories())
            {
                yield return PathFactory(dir.FullName);
            }
            foreach (var file in DirectoryInfo.GetFiles(pattern, scope))
            {
                yield return PathFactory(file.FullName);
            }
        }

        IEnumerable<IPath> IPath.ListDir(string pattern, SearchOption scope)
        {
            return ListDir(pattern, scope).Select(p => (IPath)p);
        }

        /// <inheritdoc/>
        public abstract IEnumerable<DirectoryContents<TPath>> WalkDir(Action<IOException> onError = null);

        /// <inheritdoc/>
        IEnumerable<DirectoryContents<IPath>> IPath.WalkDir(Action<IOException> onError)
        {
            return WalkDir(onError).Select(p => 
                new DirectoryContents<IPath>(p.Root, p.Directories, p.Files));
        }

        /// <inheritdoc/>
        protected abstract TPath PathFactory(params string[] path);

        /// <inheritdoc/>
        protected abstract TPath PathFactory(TPurePath path);

        /// <inheritdoc/>
        protected abstract TPath PathFactory(IPurePath path);

        /// <inheritdoc/>
        public abstract TPath Resolve();

        IPath IPath.Resolve()
        {
            return Resolve();
        }

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
                foreach (var dir in Parents())
                {
                    if(!dir.IsDir())
                    {
                        System.IO.Directory.CreateDirectory(dir.ToString());
                    }
                }
            }
            if (!IsDir())
            {
                System.IO.Directory.CreateDirectory(PurePath.ToPosix());
            }
        }

        /// <inheritdoc/>
        public void Delete(bool recursive = false)
        {
            if (!Exists())
            {
                return;
            }
            var file = FileInfo;
            if (file != null)
            {
                file.Delete();
                return;
            }
            var dir = DirectoryInfo;
            if (dir != null)
            {
                dir.Delete(recursive);
            }
        }

        /// <inheritdoc/>
        public FileStream Open(FileMode mode)
        {
            return File.Open(PurePath.ToString(), mode);
        }

        /// <inheritdoc/>
        public string ReadAsText()
        {
            return File.ReadAllText(PurePath.ToString());
        }

        /// <inheritdoc/>
        public abstract TPath ExpandUser();

        IPath IPath.ExpandUser()
        {
            return ExpandUser();
        }

        IPath IPath.ExpandUser(IPath homeDir)
        {
            return ExpandUser(homeDir);
        }

        /// <inheritdoc/>
        public TPath ExpandUser(IPath homeDir)
        {
            if (homeDir == null || PurePath.IsAbsolute())
            {
                return (TPath)this;
            }

            var parts = new List<string>();
            parts.AddRange(PurePath.Parts);
            if (parts.Count == 0 || parts[0] != "~")
            {
                return (TPath)this;
            }
            parts.RemoveAt(0);

            return PathFactory(homeDir.Join(parts.ToArray()));
        }

        IPath IPath.ExpandEnvironmentVars()
        {
            return ExpandEnvironmentVars();
        }

        /// <inheritdoc/>
        public TPath ExpandEnvironmentVars()
        {
            return PathFactory(
                Environment.ExpandEnvironmentVariables(ToString()));
        }

        /// <inheritdoc/>
        /// <inheritdoc/>
        public IDisposable SetCurrentDirectory()
        {
            return new CurrentDirectorySetter(PurePath.ToString());
        }

        private class CurrentDirectorySetter : IDisposable
        {
            private readonly string _oldCwd;
            private readonly string _newCwd;

            public CurrentDirectorySetter(string newCwd)
            {
                _oldCwd = Environment.CurrentDirectory;
                Environment.CurrentDirectory = _newCwd = newCwd;
            }
            public void Dispose()
            {
                if (Environment.CurrentDirectory == _newCwd)
                {
                    Environment.CurrentDirectory = _oldCwd;
                }
            }
        }


        #region IPurePath -> IPath implementation

        IPath IPath.Join(params string[] paths)
        {
            return Join(paths);
        }

        IPath IPath.Join(params IPurePath[] paths)
        {
            return Join(paths);
        }

        public static TPath operator/(ConcretePath<TPath, TPurePath> lvalue, IPath rvalue)
        {
            return lvalue.Join(rvalue);
        }

        public static TPath operator/(ConcretePath<TPath, TPurePath> lvalue, string rvalue)
        {
            return lvalue.Join(rvalue);
        }

        IPath IPath.NormCase()
        {
            return NormCase();
        }

        IPath IPath.NormCase(CultureInfo currentCulture)
        {
            return NormCase(currentCulture);
        }

        IPath IPath.Parent()
        {
            return Parent();
        }

        IPath IPath.Parent(int nthParent)
        {
            return Parent(nthParent);
        }

        IEnumerable<IPath> IPath.Parents()
        {
            return Parents().Select(p => (IPath) p);
        }

        IPath IPath.Relative()
        {
            return Relative();
        }

        IPath IPath.RelativeTo(IPurePath parent)
        {
            return RelativeTo(parent);
        }

        IPath IPath.WithDirname(string newDirName)
        {
            return WithDirname(newDirName);
        }

        IPath IPath.WithDirname(IPurePath newDirName)
        {
            return WithDirname(newDirName);
        }

        IPath IPath.WithFilename(string newFilename)
        {
            return WithFilename(newFilename);
        }

        IPath IPath.WithExtension(string newExtension)
        {
            return WithExtension(newExtension);
        }

        #endregion


        #region IPurePath implementation

        /// <inheritdoc/>
        public string Dirname
        {
            get { return PurePath.Dirname; }
        }

        /// <inheritdoc/>
        public string Directory
        {
            get { return PurePath.Directory; }
        }

        /// <inheritdoc/>
        public string Filename
        {
            get { return PurePath.Filename; }
        }

        /// <inheritdoc/>
        public string Basename
        {
            get { return PurePath.Basename; }
        }

        /// <inheritdoc/>
        public string BasenameWithoutExtensions
        {
            get { return PurePath.BasenameWithoutExtensions; }
        }

        /// <inheritdoc/>
        public string Extension
        {
            get { return PurePath.Extension; }
        }

        /// <inheritdoc/>
        public string[] Extensions
        {
            get { return PurePath.Extensions; }
        }

        /// <inheritdoc/>
        public string Root
        {
            get { return PurePath.Root; }
        }

        /// <inheritdoc/>
        public string Drive
        {
            get { return PurePath.Drive; }
        }

        /// <inheritdoc/>
        public string Anchor
        {
            get { return PurePath.Anchor; }
        }

        /// <inheritdoc/>
        public string ToPosix()
        {
            return PurePath.ToPosix();
        }

        /// <inheritdoc/>
        public bool IsAbsolute()
        {
            return PurePath.IsAbsolute();
        }

        /// <inheritdoc/>
        public bool IsReserved()
        {
            return PurePath.IsReserved();
        }

        /// <inheritdoc/>
        IPurePath IPurePath.Join(params string[] paths)
        {
            return PurePath.Join(paths);
        }

        /// <inheritdoc/>
        IPurePath IPurePath.Join(params IPurePath[] paths)
        {
            return PurePath.Join(paths);
        }

        /// <inheritdoc/>
        bool IPurePath.TrySafeJoin(string relativePath, out IPurePath joined)
        {
            return PurePath.TrySafeJoin(relativePath, out joined);
        }

        /// <inheritdoc/>
        bool IPurePath.TrySafeJoin(IPurePath relativePath, out IPurePath joined)
        {
            return PurePath.TrySafeJoin(relativePath, out joined);
        }

        /// <inheritdoc/>
        public bool Match(string pattern)
        {
            return PurePath.Match(pattern);
        }

        /// <inheritdoc/>
        public IEnumerable<string> Parts
        {
            get { return PurePath.Parts; }
        }

        /// <inheritdoc/>
        IPurePath IPurePath.NormCase()
        {
            return PurePath.NormCase();
        }

        /// <inheritdoc/>
        IPurePath IPurePath.NormCase(CultureInfo currentCulture)
        {
            return PurePath.NormCase(currentCulture);
        }

        /// <inheritdoc/>
        IPurePath IPurePath.Parent()
        {
            return PurePath.Parent();
        }

        /// <inheritdoc/>
        IPurePath IPurePath.Parent(int nthParent)
        {
            return PurePath.Parent(nthParent);
        }

        /// <inheritdoc/>
        IEnumerable<IPurePath> IPurePath.Parents()
        {
            return PurePath.Parents().Select(p => (IPurePath)p);
        }

        /// <inheritdoc/>
        public Uri ToUri()
        {
            return PurePath.ToUri();
        }

        /// <inheritdoc/>
        IPurePath IPurePath.Relative()
        {
            return PurePath.Relative();
        }

        /// <inheritdoc/>
        IPurePath IPurePath.RelativeTo(IPurePath parent)
        {
            return PurePath.RelativeTo(parent);
        }

        /// <inheritdoc/>
        IPurePath IPurePath.WithDirname(string newDirName)
        {
            return PurePath.WithDirname(newDirName);
        }

        /// <inheritdoc/>
        IPurePath IPurePath.WithDirname(IPurePath newDirName)
        {
            return PurePath.WithDirname(newDirName);
        }

        /// <inheritdoc/>
        IPurePath IPurePath.WithFilename(string newFilename)
        {
            return PurePath.WithFilename(newFilename);
        }

        /// <inheritdoc/>
        IPurePath IPurePath.WithExtension(string newExtension)
        {
            return PurePath.WithExtension(newExtension);
        }

        /// <inheritdoc/>
        public bool HasComponents(PathComponent components)
        {
            return PurePath.HasComponents(components);
        }

        /// <inheritdoc/>
        public string GetComponents(PathComponent components)
        {
            return PurePath.GetComponents(components);
        }


        #region IPurePath Equality

  

        #endregion


        #endregion


        #region TPurePath implementation

        /// <inheritdoc/>
        public bool TrySafeJoin(string path, out TPurePath joined)
        {
            return PurePath.TrySafeJoin(path, out joined);
        }

        /// <inheritdoc/>
        public bool TrySafeJoin(IPurePath path, out TPurePath joined)
        {
            return PurePath.TrySafeJoin(path, out joined);
        }

        /// <inheritdoc/>
        TPurePath IPurePath<TPurePath>.Join(params string[] paths)
        {
            return PurePath.Join(paths);
        }

        /// <inheritdoc/>
        TPurePath IPurePath<TPurePath>.Join(params IPurePath[] paths)
        {
            return PurePath.Join(paths);
        }

        /// <inheritdoc/>
        TPurePath IPurePath<TPurePath>.NormCase()
        {
            return PurePath.NormCase();
        }

        /// <inheritdoc/>
        TPurePath IPurePath<TPurePath>.NormCase(CultureInfo currentCulture)
        {
            return PurePath.NormCase(currentCulture);
        }

        /// <inheritdoc/>
        TPurePath IPurePath<TPurePath>.Parent()
        {
            return PurePath.Parent();
        }

        /// <inheritdoc/>
        TPurePath IPurePath<TPurePath>.Parent(int nthParent)
        {
            return PurePath.Parent(nthParent);
        }

        /// <inheritdoc/>
        IEnumerable<TPurePath> IPurePath<TPurePath>.Parents()
        {
            return PurePath.Parents();
        }

        /// <inheritdoc/>
        TPurePath IPurePath<TPurePath>.Relative()
        {
            return PurePath.Relative();
        }

        /// <inheritdoc/>
        TPurePath IPurePath<TPurePath>.RelativeTo(IPurePath parent)
        {
            return PurePath.RelativeTo(parent);
        }

        /// <inheritdoc/>
        TPurePath IPurePath<TPurePath>.WithDirname(string newDirName)
        {
            return PurePath.WithDirname(newDirName);
        }

        /// <inheritdoc/>
        TPurePath IPurePath<TPurePath>.WithDirname(IPurePath newDirName)
        {
            return PurePath.WithDirname(newDirName);
        }

        /// <inheritdoc/>
        TPurePath IPurePath<TPurePath>.WithFilename(string newFilename)
        {
            return PurePath.WithFilename(newFilename);
        }

        /// <inheritdoc/>
        TPurePath IPurePath<TPurePath>.WithExtension(string newExtension)
        {
            return PurePath.WithExtension(newExtension);
        }

        #endregion


        #region TPath implementation

        /// <inheritdoc/>
        public TPath Join(params string[] paths)
        {
            return PathFactory(PurePath.Join(paths));
        }

        /// <inheritdoc/>
        public TPath Join(params IPurePath[] paths)
        {
            return PathFactory(PurePath.Join(paths));
        }

        /// <inheritdoc/>
        public TPath NormCase()
        {
            return PathFactory(PurePath.NormCase());
        }

        /// <inheritdoc/>
        public TPath NormCase(CultureInfo currentCulture)
        {
            return PathFactory(PurePath.NormCase(currentCulture));
        }

        /// <inheritdoc/>
        public TPath Parent()
        {
            return PathFactory(PurePath.Parent());
        }

        /// <inheritdoc/>
        public TPath Parent(int nthParent)
        {
            return PathFactory(PurePath.Parent(nthParent));
        }

        /// <inheritdoc/>
        public IEnumerable<TPath> Parents()
        {
            return PurePath.Parents().Select(PathFactory);
        }

        /// <inheritdoc/>
        public TPath Relative()
        {
            return PathFactory(PurePath.Relative());
        }

        /// <inheritdoc/>
        public TPath RelativeTo(IPurePath parent)
        {
            return PathFactory(PurePath.RelativeTo(parent));
        }

        /// <inheritdoc/>
        public TPath WithDirname(string newDirName)
        {
            return PathFactory(PurePath.WithDirname(newDirName));
        }

        /// <inheritdoc/>
        public TPath WithDirname(IPurePath newDirName)
        {
            return PathFactory(PurePath.WithDirname(newDirName));
        }

        /// <inheritdoc/>
        public TPath WithFilename(string newFilename)
        {
            return PathFactory(PurePath.WithFilename(newFilename));
        }

        /// <inheritdoc/>
        public TPath WithExtension(string newExtension)
        {
            return PathFactory(PurePath.WithExtension(newExtension));
        }

        #endregion

        public bool Equals(IPath other)
        {
            return PurePath switch
            {
                null => false,
                PurePosixPath lppp when other is PosixPath rppp => lppp.Equals(rppp.PurePath),
                PureWindowsPath lpwp when other is WindowsPath rpwp => lpwp.Equals(rpwp.PurePath),
                _ => false
            };
        }
    }
}
