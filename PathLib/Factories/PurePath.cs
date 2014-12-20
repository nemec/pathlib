
namespace PathLib
{
    /// <summary>
    /// Factory functions for PurePaths
    /// </summary>
    public static class PurePath
    {
        private static readonly PurePathFactory Factory = new PurePathFactory();

        /// <summary>
        /// Any additional options to use when creating paths.
        /// </summary>
        public static PurePathFactoryOptions FactoryOptions { get; set; }

        static PurePath()
        {
            FactoryOptions = new PurePathFactoryOptions();
        }

        /// <summary>
        /// Factory method to create a new <see cref="PurePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static IPurePath Create(params string[] paths)
        {
            return Factory.Create(FactoryOptions, paths);
        }

        /// <summary>
        /// Factory method to create a new <see cref="PurePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IPurePath Create(string path)
        {
            return Factory.Create(path, FactoryOptions);
        }
    }
}
