
namespace PathLib.Extensions
{
    public static class PathExtensions
    {
        public static WindowsPath ToPath(string path)
        {
            return new WindowsPath(path);
        }
    }
}
