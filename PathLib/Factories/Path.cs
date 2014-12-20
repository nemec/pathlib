
namespace PathLib
{
    /// <summary>
    /// Factory functions for concrete paths.
    /// </summary>
    public static class Path
    {
        private static readonly PathFactory Factory = new PathFactory();

        /// <summary>
        /// Any additional options to use when creating paths.
        /// </summary>
        public static PathFactoryOptions FactoryOptions { get; set; }

        static Path()
        {
            FactoryOptions = new PathFactoryOptions();
        }

        /// <summary>
        /// Factory method to create a new <see cref="PurePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static IPath Create(params string[] paths)
        {
            return Factory.Create(FactoryOptions, paths);
        }

        /// <summary>
        /// Factory method to create a new <see cref="PurePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IPath Create(string path)
        {
            return Factory.Create(path, FactoryOptions);
        }
    }
}
