using System;

namespace PathLib
{
    /// <summary>
    /// An invalid path was specified.
    /// </summary>
    public class InvalidPathException : ArgumentException
    {
        /// <summary>
        /// An invalid path was specified.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="message"></param>
        public InvalidPathException(string path, string message)
            : base(message + " (" + path + ")") { }
    }
}
