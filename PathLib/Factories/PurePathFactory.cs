
namespace PathLib
{
    /// <summary>
    /// Creates IPurePath implementations depending on the current platform.
    /// </summary>
    public class PurePathFactory
    {
        /// <summary>
        /// Factory method to create a new <see cref="PurePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public IPurePath Create(params string[] paths)
        {
            return Create(new PurePathFactoryOptions(), paths);
        }

        /// <summary>
        /// Factory method to create a new <see cref="PurePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public IPurePath Create(PurePathFactoryOptions options, params string[] paths)
        {
            IPurePath ret = null;
            switch (PlatformChooser.GetPlatform())
            {
                case Platform.Posix:
                    ret =  new PurePosixPath(paths);
                    break;
                case Platform.Windows:
                    ret = new PureWindowsPath(paths);
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
        public IPurePath Create(string path)
        {
            return Create(path, new PurePathFactoryOptions());
        }

        /// <summary>
        /// Factory method to create a new <see cref="PurePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryCreate(string path, out IPurePath result)
        {
            return TryCreate(path, new PurePathFactoryOptions(), out result);
        }

        /// <summary>
        /// Factory method to create a new <see cref="PurePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public IPurePath Create(string path, PurePathFactoryOptions options)
        {
            IPurePath ret = null;
            switch (PlatformChooser.GetPlatform())
            {
                case Platform.Posix:
                    ret = new PurePosixPath(path);
                    break;
                case Platform.Windows:
                    ret =  new PureWindowsPath(path);
                    break;
            }
            return ApplyOptions(ret, options);
        }

        /// <summary>
        /// Factory method to create a new <see cref="PurePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryCreate(string path, PurePathFactoryOptions options, out IPurePath result)
        {
            result = null;
            switch (PlatformChooser.GetPlatform())
            {
                case Platform.Posix:
                    PurePosixPath purePosixPath;
                    if (PurePosixPath.TryParse(path, out purePosixPath))
                    {
                        result = purePosixPath;
                        break;
                    }
                    return false;
                case Platform.Windows:
                    PureWindowsPath pureWindowsPath;
                    if (PureWindowsPath.TryParse(path, out pureWindowsPath))
                    {
                        result = pureWindowsPath;
                        break;
                    }
                    return false;
            }
            result = ApplyOptions(result, options);
            return true;
        }

        private static IPurePath ApplyOptions(IPurePath path, PurePathFactoryOptions options)
        {
            if (options.AutoNormalizeCase)
            {
                path = options.Culture != null 
                    ? path.NormCase(options.Culture) 
                    : path.NormCase();
            }
            return path;
        }
    }
}
