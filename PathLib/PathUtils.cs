using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PathLib
{
    internal static class PathUtils
    {
        public static bool Glob(string pattern, string haystack, bool fullMatch = false)
        {
            var rx = Regex.Escape(pattern).Replace(@"\*", "[^/]*?").Replace(@"\?", "[^/]") + "$";
            if (fullMatch)
            {
                rx = "^" + rx;
            }
            return Regex.IsMatch(haystack, rx);
        }

        internal static string Combine(string path1, string path2, string separator)
        {
            if (path2.StartsWith(separator))
            {
                return path2;
            }
            if (path1.EndsWith(separator))
            {
                return path1 + path2;
            }
            return path1 + separator + path2;
        }

        public static IPurePath Combine(IEnumerable<IPurePath> paths, string separator)
        {
            IPurePath lastAbsolute = null;
            var dirnameBuilder = new StringBuilder();
            var lastPartStr = "";
            IPurePath lastPart  = null;

            foreach (var path in paths)
            {
                if (path.ToString() == String.Empty)
                {
                    continue;
                }

                // Does not use IsAbsolute in order to retain compatibility
                // with Path.Combine: Path.Combine("c:\\windows", "d:dir") 
                // returns "d:dir" despite the fact that it's a relative path.
                if (lastAbsolute == null || !String.IsNullOrEmpty(path.Anchor))
                {
                    dirnameBuilder.Length = 0;
                    lastAbsolute = path;
                }
                else if (dirnameBuilder.Length > 0 && !lastPartStr.EndsWith(separator))
                {
                    dirnameBuilder.Append(separator);
                }
                dirnameBuilder.Append(
                    path.GetComponents(
                        PathComponent.Dirname | PathComponent.Filename));
                lastPart = path;
                lastPartStr = path.ToString();
            }
            if (lastAbsolute == null)
            {
                return null;
            }
            var filenameLen = lastPart.Filename.Length;
            dirnameBuilder.Remove(dirnameBuilder.Length - filenameLen, filenameLen);
            return lastAbsolute
                .WithDirname(dirnameBuilder.ToString())
                .WithFilename(lastPart.Filename);
        }
    }
}
