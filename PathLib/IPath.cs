using System;
using System.Collections.Generic;
using System.IO;

namespace PathLib
{
    /// <summary>
    /// IPath implementations are platform dependent and
    /// should not be used on any platform but the one
    /// they were designed for.
    /// </summary>
    public interface IPath
    {
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
        /// Glob the given pattern in the directory 
        /// </summary>
        /// <exception cref="ArgumentException">Glob was called on a file.</exception>
        /// <param name="pattern"></param>
        /// <returns></returns>
        IEnumerable<IPath> Glob(string pattern);

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
        /// Make the path absolute, resolving any symlinks. Returns a
        /// new path.
        /// </summary>
        /// <returns></returns>
        IPath Resolve();

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
        /// Open a file pointed to by the path.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        Stream Open(FileMode mode);
    }


    /// <summary>
    /// IPath implementations are platform dependent and
    /// should not be used on any platform but the one
    /// they were designed for.
    /// </summary>
    /// <typeparam name="TPath"></typeparam>
    public interface IPath<TPath> : IPath
        where TPath : IPath
    {
        /// <summary>
        /// Glob the given pattern in the directory 
        /// </summary>
        /// <exception cref="ArgumentException">Glob was called on a file.</exception>
        /// <param name="pattern"></param>
        /// <returns></returns>
        new IEnumerable<TPath> Glob(string pattern);
        
        /// <summary>
        /// List the files in the directory.
        /// </summary>
        /// <returns></returns>
        new IEnumerable<TPath> ListDir();

        /// <summary>
        /// Make the path absolute, resolving any symlinks. Returns a
        /// new path.
        /// </summary>
        /// <returns></returns>
        new TPath Resolve();
    }
}
