
namespace PathLib
{
    public static class ConcretePath
    {
        /// <summary>
        /// Factory method to create a new <see cref="ConcretePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IPath FromString(string path)
        {
            return new WindowsPath(path);
        }

        /// <summary>
        /// Factory method to create a new <see cref="ConcretePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static IPath FromString(params string[] paths)
        {
            return new WindowsPath(paths);
        }
    }
}
