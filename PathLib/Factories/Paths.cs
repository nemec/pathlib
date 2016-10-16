using System;
using System.IO;

namespace PathLib
{
    /// <summary>
    /// Factory functions for concrete paths.
    /// </summary>
    public static class Paths
    {
        private static readonly PathFactory Factory = new PathFactory();

        /// <summary>
        /// Any additional options to use when creating paths.
        /// </summary>
        public static PathFactoryOptions FactoryOptions { get; set; }

        static Paths()
        {
            FactoryOptions = new PathFactoryOptions
            {
                AutoExpandUserDirectory = true
            };
        }

        /// <summary>
        /// Factory method to create a new <see cref="Path"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static IPath Create(params string[] paths)
        {
            return Factory.Create(FactoryOptions, paths);
        }

        /// <summary>
        /// Factory method to create a new <see cref="Path"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IPath Create(string path)
        {
            return Factory.Create(path, FactoryOptions);
        }

        /// <summary>
        /// Factory method to create a new <see cref="Path"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static IPath Create(FileInfo info)
        {
            return Factory.Create(info.FullName);
        }

        /// <summary>
        /// Factory method to create a new <see cref="Path"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static IPath Create(DirectoryInfo info)
        {
            return Factory.Create(info.FullName);
        }

        /// <summary>
        /// Gets the fully-qualified path of the working directory as an IPath.
        /// </summary>
        public static IPath CurrentDirectory
        {
            get { return Factory.Create(Environment.CurrentDirectory, FactoryOptions); }
        }

        /// <summary>
        /// Gets the fully-qualified path of the system directory as an IPath.
        /// </summary>
        public static IPath SystemDirectory
        {
            get { return Factory.Create(Environment.SystemDirectory, FactoryOptions); }
        }
    }
}
