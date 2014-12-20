
namespace PathLib
{
    /// <summary>
    /// Creates IPurePath implementations depending on the current platform.
    /// </summary>
    public class PathFactory
    {
        /// <summary>
        /// Factory method to create a new <see cref="PurePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public IPath Create(params string[] paths)
        {
            return Create(new PathFactoryOptions(), paths);
        }

        /// <summary>
        /// Factory method to create a new <see cref="PurePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public IPath Create(PathFactoryOptions options, params string[] paths)
        {
            IPath ret = null;
            switch (PlatformChooser.GetPlatform())
            {
                case Platform.Posix:
                    ret =  new PosixPath(paths);
                    break;
                case Platform.Windows:
                    ret = new WindowsPath(paths);
                    break;
            }
            return ApplyOptions(ret, options);
        }

        /// <summary>
        /// Factory method to create a new <see cref="PurePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public IPath Create(string path)
        {
            return Create(path, new PathFactoryOptions());
        }

        /// <summary>
        /// Factory method to create a new <see cref="PurePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryCreate(string path, out IPath result)
        {
            return TryCreate(new PathFactoryOptions(), path, out result);
        }

        /// <summary>
        /// Factory method to create a new <see cref="PurePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public IPath Create(string path, PathFactoryOptions options)
        {
            IPath ret = null;
            switch (PlatformChooser.GetPlatform())
            {
                case Platform.Posix:
                    ret = new PosixPath(path);
                    break;
                case Platform.Windows:
                    ret =  new WindowsPath(path);
                    break;
            }
            return ApplyOptions(ret, options);
        }

        /// <summary>
        /// Factory method to create a new <see cref="PurePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="path"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryCreate(PathFactoryOptions options, string path, out IPath result)
        {
            result = null;
            switch (PlatformChooser.GetPlatform())
            {
                case Platform.Posix:
                    PurePosixPath purePosixPath;
                    if (PurePosixPath.TryParse(path, out purePosixPath))
                    {
                        result = new PosixPath(purePosixPath);
                        break;
                    }
                    return false;
                case Platform.Windows:
                    PureWindowsPath pureWindowsPath;
                    if (PureWindowsPath.TryParse(path, out pureWindowsPath))
                    {
                        result = new WindowsPath(path);
                        break;
                    }
                    return false;
            }
            result = ApplyOptions(result, options);
            return true;
        }

        private static IPath ApplyOptions(IPath path, PathFactoryOptions options)
        {
            if (options.AutoNormalizeCase)
            {
                path = options.Culture != null 
                    ? path.NormCase(options.Culture) 
                    : path.NormCase();
            }
            if (options.AutoExpandEnvironmentVariables)
            {
                path = path.ExpandEnvironmentVars();
            }
            if (options.AutoExpandUserDirectory)
            {
                path = options.UserDirectory != null 
                    ? path.ExpandUser(options.UserDirectory) 
                    : path.ExpandUser();
            }
            return path;
        }
    }
}
