using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace PathLib
{
    /// <summary>
    /// Pure paths do not implement any IO operations and may be
    /// used cross-platform.
    /// </summary>
    [TypeConverter(typeof(PurePathFactoryConverter))]
    public interface IPurePath
    {
        /// <summary>
        /// Get the path's directory (between the root and the file).
        /// </summary>
        string Dirname { get; }

        /// <summary>
        /// Get the path's full directory name (including root). For
        /// those expecting a similar output as
        /// <see cref="System.IO.Path.GetDirectoryName"/>.
        /// </summary>
        string Directory { get; }

        /// <summary>
        /// Get the path's filename (basename + extension).
        /// </summary>
        string Filename { get; }

        /// <summary>
        /// Returns the filename minus the final extension.
        /// </summary>
        string Basename { get; }

        /// <summary>
        /// Returns the filename minus all extensions.
        /// </summary>
        string BasenameWithoutExtensions { get; }

        /// <summary>
        /// <para>Gets the path's last extension.</para>
        /// <para>Extensions are prepended with a '.'</para>
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// <para>
        /// Get each of the path's extensions, in order from first to last
        /// (inner to outer).
        /// </para>
        /// <para>
        /// Extensions are prepended with a '.'
        /// </para>
        /// </summary>
        string[] Extensions { get; }

        /// <summary>
        /// Gets the path 
        /// </summary>
        string Root { get; }

        /// <summary>
        /// The drive value of the path (eg. c:).
        /// Empty string on Posix machines.
        /// </summary>
        string Drive { get; }

        /// <summary>
        /// Concatenation of drive and root (eg. c:\)
        /// </summary>
        string Anchor { get; }

        /// <summary>
        /// Return the string representation of the path as a Posix
        /// path (eg. using forward slashes).
        /// </summary>
        /// <returns>Posix string representation of the path.</returns>
        string ToPosix();

        /// <summary>
        /// Returns true if the given path is absolute.
        /// </summary>
        /// <returns></returns>
        bool IsAbsolute();

        /// <summary>
        /// Return true if the path is considered reserved (on Windows). Other
        /// platforms do not have reserved paths. Filesystem calls on reserved
        /// paths may fail mysteriously or have unintended effects.
        /// </summary>
        /// <returns></returns>
        bool IsReserved();

        /// <summary>
        /// Join the current path with the provided paths, in turn.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        IPurePath Join(params string[] paths);

        /// <summary>
        /// Join the current path with the provided paths, in turn.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        IPurePath Join(params IPurePath[] paths);

        /// <summary>
        /// Join the current path with the provided path. In addition,
        /// the joined path is not allowed to traverse outside the directory
        /// of the original path. After combining, the result is checked to 
        /// ensure the first N characters (where N is the length of the
        /// original path) is byte-for-byte equal to the original.
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="joined"></param>
        /// <returns></returns>
        bool TrySafeJoin(string relativePath, out IPurePath joined);

        /// <summary>
        /// Join the current path with the provided path. In addition,
        /// the joined path is not allowed to traverse outside the directory
        /// of the original path. After combining, the result is checked to 
        /// ensure the first N characters (where N is the length of the
        /// original path) is byte-for-byte equal to the original.
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="joined"></param>
        /// <returns></returns>
        bool TrySafeJoin(IPurePath relativePath, out IPurePath joined);

        /// <summary>
        /// <para>
        /// Test whether the path matches the given
        /// glob pattern (* and ? are wildcards).
        /// </para>
        /// <para>
        /// If pattern is relative, path can be either
        /// relative or absolute. If absolute, the whole
        /// path must match.
        /// </para>
        /// <para>
        /// Matching is case insensitive on NT machines.
        /// </para>
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        bool Match(string pattern);

        /// <summary>
        /// Iterator through the individual directory/file parts of
        /// the path (the parts separated by the path separator).
        /// </summary>
        IEnumerable<string> Parts { get; }

        /// <summary>
        /// Normalize the case of the path using the current program culture.
        /// On case-insensitive platforms, will return the path lowercased,
        /// otherwise the path is returned unchanged.
        /// </summary>
        /// <returns></returns>
        IPurePath NormCase();

        /// <summary>
        /// Normalize the case of the path. On case-insensitive platforms,
        /// will return the path lowercased, otherwise the path is returned
        /// unchanged.
        /// </summary>
        /// <returns></returns>
        IPurePath NormCase(CultureInfo currentCulture);

        /// <summary>
        /// Returns the parent directory of the current path.
        /// </summary>
        /// <returns></returns>
        IPurePath Parent();

        /// <summary>
        /// Returns the nth parent directory of the current path.
        /// </summary>
        /// <param name="nthParent"></param>
        /// <returns></returns>
        IPurePath Parent(int nthParent);

        /// <summary>
        /// Iterate over the path's parents from most to least specific.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IPurePath> Parents();

        /// <summary>
        /// Return the current path as a URI.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if attempting to create a URI from a relative path.
        /// </exception>
        /// <returns></returns>
        Uri ToUri();

        /// <summary>
        /// Return the page stripped of its drive and root, if any.
        /// </summary>
        /// <returns></returns>
        IPurePath Relative();

        /// <summary>
        /// Compute a version of this path relative to the
        /// path represented by "parent".
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        IPurePath RelativeTo(IPurePath parent);

        /// <summary>
        /// Returns a new path with the directory name changed.
        /// The drive, root, and filename all stay the same.
        /// </summary>
        /// <param name="newDirName"></param>
        /// <returns></returns>
        IPurePath WithDirname(string newDirName);

        /// <summary>
        /// Returns a new path with the directory name changed.
        /// The drive, root, and filename all stay the same.
        /// </summary>
        /// <param name="newDirName"></param>
        /// <returns></returns>
        IPurePath WithDirname(IPurePath newDirName);

        /// <summary>
        /// Returns a new path with filename changed.
        /// </summary>
        /// <param name="newFilename"></param>
        /// <returns></returns>
        IPurePath WithFilename(string newFilename);

        /// <summary>
        /// Changes the extension of the current path.
        /// </summary>
        /// <param name="newExtension"></param>
        /// <returns></returns>
        IPurePath WithExtension(string newExtension);

        /// <summary>
        /// Return true if the path contains any of the specified components.
        /// </summary>
        /// <param name="components"></param>
        /// <returns></returns>
        bool HasComponents(PathComponent components);

        /// <summary>
        /// Return a string composed of all specified components.
        /// </summary>
        /// <param name="components">
        /// The specified component(s).
        /// </param>
        /// <returns></returns>
        string GetComponents(PathComponent components);
    }

    /// <summary>
    /// Pure paths do not implement any IO operations and may be
    /// used cross-platform.
    /// </summary>
    public interface IPurePath<TPath> : IPurePath
        where TPath : IPurePath
    {

        /// <summary>
        /// Join the current path with the provided paths, in turn.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        new TPath Join(params string[] paths);

        /// <summary>
        /// Join the current path with the provided paths, in turn.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        new TPath Join(params IPurePath[] paths);

        /// <summary>
        /// Join the current path with the provided path. In addition,
        /// the joined path is not allowed to traverse outside the directory
        /// of the original path. After combining, the result is checked to 
        /// ensure the first N characters (where N is the length of the
        /// original path) is byte-for-byte equal to the original.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="joined"></param>
        /// <returns></returns>
        bool TrySafeJoin(string path, out TPath joined);

        /// <summary>
        /// Join the current path with the provided path. In addition,
        /// the joined path is not allowed to traverse outside the directory
        /// of the original path. After combining, the result is checked to 
        /// ensure the first N characters (where N is the length of the
        /// original path) is byte-for-byte equal to the original.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="joined"></param>
        /// <returns></returns>
        bool TrySafeJoin(IPurePath path, out TPath joined);

        /// <summary>
        /// Normalize the case of the path using the current platform culture. 
        /// On case-insensitive platforms, will return the path lowercased,
        /// otherwise the path is returned unchanged.
        /// </summary>
        /// <returns></returns>
        new TPath NormCase();

        /// <summary>
        /// Normalize the case of the path. On case-insensitive platforms,
        /// will return the path lowercased, otherwise the path is returned
        /// unchanged.
        /// </summary>
        /// <returns></returns>
        new TPath NormCase(CultureInfo currentCulture);

        /// <summary>
        /// Returns the parent directory of the current path.
        /// </summary>
        /// <returns></returns>
        new TPath Parent();

        /// <summary>
        /// Returns the nth parent directory of the current path.
        /// </summary>
        /// <param name="nthParent"></param>
        /// <returns></returns>
        new TPath Parent(int nthParent);

        /// <summary>
        /// Iterate over the path's parents from most to least specific.
        /// </summary>
        /// <returns></returns>
        new IEnumerable<TPath> Parents();

        /// <summary>
        /// Return the page stripped of its drive and root, if any.
        /// </summary>
        /// <returns></returns>
        new TPath Relative();

        /// <summary>
        /// Compute a version of this path relative to the
        /// path represented by "parent".
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        new TPath RelativeTo(IPurePath parent);

        /// <summary>
        /// Returns a new path with the directory name changed.
        /// The drive, root, and filename all stay the same.
        /// </summary>
        /// <param name="newDirName"></param>
        /// <returns></returns>
        new TPath WithDirname(string newDirName);

        /// <summary>
        /// Returns a new path with the directory name changed.
        /// The drive, root, and filename all stay the same.
        /// </summary>
        /// <param name="newDirName"></param>
        /// <returns></returns>
        new TPath WithDirname(IPurePath newDirName);

        /// <summary>
        /// Returns a new path with filename changed.
        /// </summary>
        /// <param name="newFilename"></param>
        /// <returns></returns>
        new TPath WithFilename(string newFilename);

        /// <summary>
        /// Changes the extension of the current path.
        /// </summary>
        /// <param name="newExtension"></param>
        /// <returns></returns>
        new TPath WithExtension(string newExtension);
    }
}
