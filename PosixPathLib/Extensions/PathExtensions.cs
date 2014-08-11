
namespace PathLib.Extensions
{
    public static class PathExtensions
    {
        public static PosixPath ToPath(string path)
        {
            return new PosixPath(path);
        }
    }
}
