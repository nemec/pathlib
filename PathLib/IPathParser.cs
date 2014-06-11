
namespace PathLib
{
    /// <summary>
    /// Parses values out of a path string.
    /// </summary>
    public interface IPathParser
    {
        /// <summary>
        /// Indicates a reserved character is in the path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="reservedCharacter"></param>
        /// <returns></returns>
        bool ReservedCharactersInPath(string path, out char reservedCharacter);

        /// <summary>
        /// Parses a drive letter out of a path.
        /// </summary>
        /// <param name="remainingPath"></param>
        /// <returns></returns>
        string ParseDrive(string remainingPath);

        /// <summary>
        /// Parses the root out of a path.
        /// </summary>
        /// <param name="remainingPath"></param>
        /// <returns></returns>
        string ParseRoot(string remainingPath);

        /// <summary>
        /// Parses the dirname out of a path.
        /// </summary>
        /// <param name="remainingPath"></param>
        /// <returns></returns>
        string ParseDirname(string remainingPath);

        /// <summary>
        /// Parses a basename out of a path.
        /// </summary>
        /// <param name="remainingPath"></param>
        /// <returns></returns>
        string ParseBasename(string remainingPath);

        /// <summary>
        /// Parses an extension out of a path.
        /// </summary>
        /// <param name="remainingPath"></param>
        /// <returns></returns>
        string ParseExtension(string remainingPath);
    }
}
