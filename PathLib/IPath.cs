using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace PathLib
{
    /// <summary>
    /// IPath implementations are platform dependent and
    /// should not be used on any platform but the one
    /// they were designed for.
    /// </summary>
    [TypeConverter(typeof(PathFactoryConverter))]
    public interface IPath : IPurePath
    {
        /// <summary>
        /// The size of the file.
        /// </summary>
        FileSize Size { get; }

        /// <summary>
        /// Returns information about the path.
        /// Information is cached indefinitely.
        /// </summary>
        /// <returns></returns>
        StatInfo Stat();

        /// <summary>
        /// Returns information about the path, discarding
        /// any cached information. Next call to stat will
        /// return this value.
        /// </summary>
        /// <returns></returns>
        StatInfo Restat();

        /// <summary>
        /// Retrieve the FileInfo object for this path or null
        /// if exists and not a file.
        /// </summary>
        FileInfo FileInfo { get; }

        /// <summary>
        /// Retrieve the DirectoryInfo object for this path or null
        /// if exists and not a file.
        /// </summary>
        DirectoryInfo DirectoryInfo { get; }

        /// <summary>
        /// Change file mode and permissions.
        /// Like a Unix chmod.
        /// </summary>
        /// <param name="mode"></param>
        void Chmod(int mode);

        /// <summary>
        /// Return true if the path exists.
        /// </summary>
        /// <returns></returns>
        bool Exists();

        /// <summary>
        /// Return true if the path is a directory.
        /// </summary>
        /// <returns></returns>
        bool IsDir();

        /// <summary>
        /// List the files in the directory.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IPath> ListDir();

        /// <summary>
        /// Glob the given pattern in the directory 
        /// </summary>
        /// <exception cref="ArgumentException">
        ///     The path being globbed is a file, not a directory.
        /// </exception>
        /// <param name="pattern">
        /// A pattern to match. The special character '*' will match any
        /// number of characters while '?' will match one character.
        /// </param>
        /// <returns></returns>
        IEnumerable<IPath> ListDir(string pattern);

        /// <summary>
        /// Glob the given pattern in the directory, with the specified scope.
        /// </summary>
        /// <exception cref="ArgumentException">Glob was called on a file.</exception>
        /// <param name="scope">Whether to search in subdirectories or not.</param>
        /// <returns></returns>
        IEnumerable<IPath> ListDir(SearchOption scope);

        /// <summary>
        /// Glob the given pattern in the directory, with the specified scope.
        /// </summary>
        /// <exception cref="ArgumentException">Glob was called on a file.</exception>
        /// <param name="pattern">
        /// A pattern to match. The special character '*' will match any
        /// number of characters while '?' will match one character.
        /// </param>
        /// <param name="scope">Whether to search in subdirectories or not.</param>
        /// <returns></returns>
        IEnumerable<IPath> ListDir(string pattern, SearchOption scope);

        /// <summary>
        /// Generate the files names in a directory tree.
        /// </summary>
        /// <returns></returns>
        IEnumerable<DirectoryContents<IPath>> WalkDir(Action<IOException> onError = null);

        // TODO OS.Walk

        /// <summary>
        /// Make the path absolute, resolving any symlinks. Returns a
        /// new path.
        /// </summary>
        /// <returns></returns>
        IPath Resolve();

        // TODO ResolveGlob - evaluate the path and transform any globbed parts into paths

        /// <summary>
        /// Return true if the path is a file.
        /// </summary>
        /// <returns></returns>
        bool IsFile();

        /// <summary>
        /// Return true if the path points to a symbolic link.
        /// </summary>
        /// <returns></returns>
        bool IsSymlink();

        /// <summary>
        /// Like <see cref="Chmod"/>, but if the path points to
        /// a symbolic link the link's mode is changed rather than
        /// its target's.
        /// </summary>
        /// <param name="mode"></param>
        void Lchmod(int mode);

        /// <summary>
        /// Like <see cref="Stat"/>, but if the path points to
        /// a symbolic link the link's information is returned
        /// rather than its target's.
        /// </summary>
        /// <returns></returns>
        StatInfo Lstat();

        /// <summary>
        /// Create a new directory at the given path.
        /// </summary>
        /// <param name="makeParents"></param>
        void Mkdir(bool makeParents = false);

        /// <summary>
        /// Delete the file or directory represented by the path.
        /// If a directory, recursively delete all child files too.
        /// </summary>
        /// <param name="recursive"></param>
        void Delete(bool recursive = false);

        /// <summary>
        /// Open a file pointed to by the path.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        FileStream Open(FileMode mode);

        /// <summary>
        /// Reads the file and returns its contents in a string.
        /// </summary>
        /// <returns></returns>
        string ReadAsText();

        /// <summary>
        /// Expand a leading ~ into the user's home directory.
        /// </summary>
        /// <returns></returns>
        IPath ExpandUser();

        /// <summary>
        /// Expand a leading ~ into the given home directory.
        /// </summary>
        /// <param name="homeDir"></param>
        /// <returns></returns>
        IPath ExpandUser(IPath homeDir);

        /// <summary>
        /// Expand all environment variables in the path.
        /// </summary>
        /// <returns></returns>
        IPath ExpandEnvironmentVars();

        /// <summary>
        /// Set the current working directory to this path. Upon dispose,
        /// resets to the original working directory only if the current
        /// directory has not been changed in the meantime.
        /// </summary>
        /// <returns></returns>
        IDisposable SetCurrentDirectory();


        #region IPurePath override

        /// <inheritdoc/>
        new IPath Join(params string[] paths);

        /// <inheritdoc/>
        new IPath Join(params IPurePath[] paths);

        /// <inheritdoc/>
        new IPath NormCase();

        /// <inheritdoc/>
        new IPath NormCase(CultureInfo currentCulture);

        /// <inheritdoc/>
        new IPath Parent();

        /// <inheritdoc/>
        new IPath Parent(int nthParent);

        /// <inheritdoc/>
        new IEnumerable<IPath> Parents();

        /// <inheritdoc/>
        new IPath Relative();

        /// <inheritdoc/>
        new IPath RelativeTo(IPurePath parent);

        /// <inheritdoc/>
        new IPath WithDirname(string newDirName);

        /// <inheritdoc/>
        new IPath WithDirname(IPurePath newDirName);

        /// <inheritdoc/>
        new IPath WithFilename(string newFilename);

        /// <inheritdoc/>
        new IPath WithExtension(string newExtension);

        #endregion
    }


    /// <summary>
    /// IPath implementations are platform dependent and
    /// should not be used on any platform but the one
    /// they were designed for.
    /// </summary>
    /// <typeparam name="TPath"></typeparam>
    /// <typeparam name="TPurePath"></typeparam>
    public interface IPath<TPath, TPurePath> : IPath , IPurePath<TPurePath>
        where TPath : IPath
        where TPurePath : IPurePath
    {
        /// <summary>
        /// List the files in the directory.
        /// </summary>
        /// <returns></returns>
        new IEnumerable<TPath> ListDir();

        /// <summary>
        /// Glob the given pattern in the directory 
        /// </summary>
        /// <exception cref="ArgumentException">Glob was called on a file.</exception>
        /// <param name="pattern"></param>
        /// <returns></returns>
        new IEnumerable<TPath> ListDir(string pattern);

        /// <summary>
        /// Make the path absolute, resolving any symlinks. Returns a
        /// new path.
        /// </summary>
        /// <returns></returns>
        new TPath Resolve();

        /// <summary>
        /// Expand a leading ~ into the user's home directory.
        /// </summary>
        /// <returns></returns>
        new TPath ExpandUser();

        /// <summary>
        /// Expand a leading ~ into the given home directory.
        /// </summary>
        /// <param name="homeDir"></param>
        /// <returns></returns>
        new TPath ExpandUser(IPath homeDir);

        /// <summary>
        /// Expand all environment variables in the path.
        /// </summary>
        /// <returns></returns>
        new TPath ExpandEnvironmentVars();


        #region IPurePath override

        /// <inheritdoc/>
        new TPath Join(params string[] paths);

        /// <inheritdoc/>
        new TPath Join(params IPurePath[] paths);

        /// <inheritdoc/>
        new TPath NormCase();

        /// <inheritdoc/>
        new TPath NormCase(CultureInfo currentCulture);

        /// <inheritdoc/>
        new TPath Parent();

        /// <inheritdoc/>
        new TPath Parent(int nthParent);

        /// <inheritdoc/>
        new IEnumerable<TPath> Parents();

        /// <inheritdoc/>
        new TPath Relative();

        /// <inheritdoc/>
        new TPath RelativeTo(IPurePath parent);

        /// <inheritdoc/>
        new TPath WithDirname(string newDirName);

        /// <inheritdoc/>
        new TPath WithDirname(IPurePath newDirName);

        /// <inheritdoc/>
        new TPath WithFilename(string newFilename);

        /// <inheritdoc/>
        new TPath WithExtension(string newExtension);

        #endregion
    }
}
