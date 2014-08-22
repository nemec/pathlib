
using System;

namespace PathLib
{
    /// <summary>
    /// Represents an individual component in the path.
    /// </summary>
    [Flags]
    public enum PathComponent
    {
        /// <summary>
        /// Path's drive on platforms that support a drive.
        /// </summary>
        Drive = 1 << 1,

        /// <summary>
        /// The root of the drive, if absolute path.
        /// </summary>
        Root = 1 << 2,

        /// <summary>
        /// Directory name.
        /// </summary>
        Dirname = 1 << 3,

        /// <summary>
        /// The filename minus the extension.
        /// </summary>
        Basename = 1 << 4,

        /// <summary>
        /// File extension.
        /// </summary>
        Extension = 1 << 5,

        /// <summary>
        /// The combination of <see cref="Drive"/> and <see cref="Root"/>.
        /// </summary>
        Anchor = Drive | Root,

        /// <summary>
        /// The combination of <see cref="Basename"/> and
        /// <see cref="Filename"/>.
        /// </summary>
        Filename = Basename | Extension,

        /// <summary>
        /// All path components.
        /// </summary>
        All = Drive | Root | Dirname | Basename | Extension
    }
}
