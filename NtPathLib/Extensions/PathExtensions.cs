
namespace PathLib.Extensions
{
    public static class PathExtensions
    {
        public static NtPath ToPath(string path)
        {
            return new NtPath(path);
        }
    }
}
