
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
        /// </summary>
        Drive = 1 << 1,

        /// <summary>
        /// </summary>
        Root = 1 << 2,

        /// <summary>
        /// </summary>
        Dirname = 1 << 3,

        /// <summary>
        /// The filename minus the extension.
        /// </summary>
        Basename = 1 << 4,

        /// <summary>
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
