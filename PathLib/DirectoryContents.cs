using System.Collections.Generic;

namespace PathLib
{
    /// <summary>
    /// Contents of a directory
    /// </summary>
    public class DirectoryContents<TPath>
        where TPath : IPath
    {
        /// <summary>
        /// The path to this directory.
        /// </summary>
        public TPath Root { get; }

        /// <summary>
        /// The directories within the root directory.
        /// </summary>
        public IList<IPath> Directories { get; }

        /// <summary>
        /// The files within the root directory.
        /// </summary>
        public IList<IPath> Files { get; }

        /// <summary>
        /// Contents of a directory
        /// </summary>
        public DirectoryContents(TPath root)
        {
            Root = root;
            Directories = new List<IPath>();
            Files = new List<IPath>();
        }

        internal DirectoryContents(TPath root, IList<IPath> dirs, IList<IPath> files)
        {
            Root = root;
            Directories = dirs;
            Files = files;
        }
    }
}
