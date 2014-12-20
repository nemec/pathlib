
using System.Globalization;

namespace PathLib
{
    /// <summary>
    /// Construction options for pure path factories.
    /// </summary>
    public class PurePathFactoryOptions
    {
        /// <summary>
        /// Optional culture to use for initialization.
        /// </summary>
        public CultureInfo Culture { get; set; }

        /// <summary>
        /// Always normalize the path's case when created
        /// by this factory.
        /// </summary>
        public bool AutoNormalizeCase { get; set; }
    }

    /// <summary>
    /// Construction options for concrete path factories.
    /// </summary>
    public class PathFactoryOptions
    {
        /// <summary>
        /// Optional culture to use for initialization.
        /// </summary>
        public CultureInfo Culture { get; set; }

        /// <summary>
        /// Optional alternate user directory to use when
        /// expanding the home directory.
        /// </summary>
        public IPath UserDirectory { get; set; }

        /// <summary>
        /// Always expand all environment variables for
        /// paths created by this factory.
        /// </summary>
        public bool AutoExpandEnvironmentVariables { get; set; }

        /// <summary>
        /// Always expand the home directory (~) for paths
        /// created by this factory.
        /// </summary>
        public bool AutoExpandUserDirectory { get; set; }

        /// <summary>
        /// Always normalize the path's case when created
        /// by this factory.
        /// </summary>
        public bool AutoNormalizeCase { get; set; }
    }
}
