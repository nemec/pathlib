using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

// Certain path methods adapted from Mono source to use 
// specific directory separator: System.IO.Path.cs
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace PathLib.Utils
{
    /// <summary>
    /// General utilities and separator-agnostic replacements for
    /// System.IO.Path methods.
    /// </summary>
    internal static class PathUtils
    {
        /// <summary>
        /// A string representing the operating system's "current directory"
        /// identifier.
        /// </summary>
        public const string CurrentDirectoryIdentifier = ".";

        /// <summary>
        /// A string representing the operating system's "parent directory"
        /// identifier.
        /// </summary>
        public const string ParentDirectoryIdentifier = "..";

        /// <summary>
        /// A char representing the character delimiting extensions
        /// from filenames.
        /// </summary>
        public const char ExtensionDelimiter = '.';

        /// <summary>
        /// A string representing the character delimiting drives from the
        /// remaining path.
        /// </summary>
        public const char DriveDelimiter = ':';
        internal static readonly string[] PathSeparatorsForNormalization = {
            "/",
            @"\"
        };

        internal static bool Glob(string pattern, string haystack,
            bool fullMatch = false, bool caseInsensitive = false)
        {
            var rx = Regex.Escape(pattern)
                .Replace(@"\*", "[^/]*?")
                .Replace(@"\?", "[^/]") + "$";
            if(fullMatch)
            {
                rx = "^" + rx;
            }
            return Regex.IsMatch(haystack, rx, caseInsensitive 
                                                ? RegexOptions.IgnoreCase 
                                                : RegexOptions.None);
        }

        internal static string Combine(string path1, string path2, string separator)
        {
            if(path2.StartsWith(separator))
            {
                return path2;
            }
            if(path1.EndsWith(separator))
            {
                return path1 + path2;
            }
            return path1 + separator + path2;
        }

        internal static IPurePath Combine(IEnumerable<IPurePath> paths, string separator)
        {
            if (String.IsNullOrEmpty(separator))
            {
                throw new ArgumentException(
                    "Separator cannot be empty or null.", "separator");
            }
            IPurePath lastAbsolute = null;
            var dirnameBuilder = new StringBuilder();
            var lastPartStr = "";
            IPurePath lastPart = null;

            foreach (var path in paths)
            {
                var pStr = path.ToString();
                if(pStr == String.Empty || pStr == CurrentDirectoryIdentifier)
                {
                    continue;
                }

                // Does not use IsAbsolute in order to retain compatibility
                // with Path.Combine: Path.Combine("c:\\windows", "d:dir") 
                // returns "d:dir" despite the fact that it's a relative path.
                if(lastAbsolute == null || !String.IsNullOrEmpty(path.Anchor))
                {
                    dirnameBuilder.Length = 0;
                    lastAbsolute = path;
                }
                else if(dirnameBuilder.Length > 0 && !lastPartStr.EndsWith(separator))
                {
                    dirnameBuilder.Append(separator);
                }
                
                dirnameBuilder.Append(
                    path.GetComponents(PathComponent.Dirname | PathComponent.Filename)); 
                lastPart = path;
                lastPartStr = path.ToString();
            }
            if(lastAbsolute == null)
            {
                return null;
            }

            var filenameLen = lastPart.Filename.Length;
            dirnameBuilder.Remove(dirnameBuilder.Length - filenameLen, filenameLen);
            // TODO optimize this so fewer new objects are created
            return lastAbsolute
                .WithDirname(dirnameBuilder
                    .ToString()
                    .TrimEnd(separator[0]))
                .WithFilename(lastPart.Filename);
        }

        /// <summary>
        /// Removes all parent directory components. Disallows combining
        /// with absolute paths.
        /// </summary>
        /// <param name="base"></param>
        /// <param name="toJoin"></param>
        /// <param name="separator"></param>
        /// <param name="combined"></param>
        /// <returns></returns>
        internal static bool TrySafeCombine(
            IPurePath @base, IPurePath toJoin, string separator, out string combined)
        {
            var joinParts = new List<string>();
            foreach (var part in toJoin.Parts)
            {
                if (part == toJoin.Anchor)
                {
                    continue;
                }
                var normalized = part.Normalize(NormalizationForm.FormKC);
                if (normalized == ParentDirectoryIdentifier)
                {
                    if (joinParts.Count > 0)
                    {
                        joinParts.RemoveAt(joinParts.Count - 1);
                    }
                    else  // attempted parent dir
                    {
                        combined = null;
                        return false;
                    }
                }
                else
                {
                    joinParts.Add(part);
                }
            }

            var parts = new List<string>(@base.Parts);
            parts.AddRange(joinParts);

            combined = String.Join(separator, parts.ToArray());
            return true;
        }


        #region System.IO.Path replacements
        // ReSharper disable CSharpWarnings::CS1591
        // ReSharper disable StringLastIndexOfIsCultureSpecific.1
        // ReSharper disable RedundantIfElseBlock
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Returns the root portion of the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string GetPathRoot(string path, string separator)
        {
            if(!IsPathRooted(path, separator))
            {
                return String.Empty;
            }
            if(separator.Length != 1)
            {
                throw new ArgumentException("Separator must be one character", "separator");
            }

            if(separator == "/")
            {
                // UNIX
                return path[0] == separator[0] ? separator : String.Empty;
            }
            else
            {
                // Windows
                int len = 2;

                if(path.Length == 1 && path[0] == separator[0])
                {
                    return separator;
                }
                else if(path.Length < 2)
                {
                    return String.Empty;
                }

                if(path[0] == separator[0] && path[1] == separator[0])
                {
                    // UNC: \\server or \\server\share
                    // Get server
                    while(len < path.Length && path[len] != separator[0])
                        len++;

                    // Get share
                    if(len < path.Length)
                    {
                        len++;
                        while(len < path.Length && path[len] != separator[0])
                            len++;
                    }
                    return separator + separator + path.Substring(2, len - 2);
                }
                else if(path[0] == separator[0])
                {
                    // path starts with '\' or '/'
                    return separator;
                }
                else if(path[1] == DriveDelimiter)
                {
                    // C:\folder
                    if(path.Length >= 3 && path[2] == separator[0])
                        len++;
                }
                return path.Substring(0, len);
            }
        }

        /// <summary>
        /// Tests if the given path contains a root.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static bool IsPathRooted(string path, string separator)
        {
            if(string.IsNullOrEmpty(path))
            {
                return false;
            }
            if(separator.Length != 1)
            {
                throw new ArgumentException("Separator must be one character", "separator");
            }

            char c = path[0];
            return (c == separator[0] ||
                (path.Length > 1 && path[1] == DriveDelimiter));
        }

        /// <summary>
        /// Returns the directory path of a file path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string GetDirectoryName(string path, string separator)
        {
            if(path == null || GetPathRoot(path, separator) == path)
            {
                return null;
            }
            if(separator.Length != 1)
            {
                throw new ArgumentException("Separator must be one character", "separator");
            }

            int nLast = path.LastIndexOf(separator);
            if(nLast == 0)
            {
                nLast++;
            }

            if(nLast > 0)
            {
                // Trim multiple separators in a row
                while (nLast - 1 >= 0 && path[nLast - 1] == separator[0])
                {
                    nLast--;
                }
                string ret = path.Substring(0, nLast);
                int l = ret.Length;

                if(l >= 2 && separator[0] == '\\' && ret[l - 1] == DriveDelimiter)
                {
                    return ret + separator;
                }
                else if(l == 1 && separator[0] == '\\' && path.Length >= 2 && path[nLast] == DriveDelimiter)
                {
                    return ret + DriveDelimiter;
                }
                else
                {
                    return ret;
                }
            }
            return String.Empty;
        }

        /// <summary>
        /// Changes the extension of a file path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="extension"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string ChangeExtension(string path, string extension, string separator)
        {
            if(path == null)
                return null;

            int iExt = findExtension(path, separator);

            if(extension == null)
            {
                return iExt < 0 ? path : path.Substring(0, iExt);
            }
            else if(extension.Length == 0)
            {
                return iExt < 0 ? path + '.' : path.Substring(0, iExt + 1);
            }
            else if(path.Length != 0)
            {
                if(extension.Length > 0 && extension[0] != '.')
                {
                    extension = "." + extension;
                }
            }
            else
            {
                extension = String.Empty;
            }

            if(iExt < 0)
            {
                return path + extension;
            }
            else if(iExt > 0)
            {
                string temp = path.Substring(0, iExt);
                return temp + extension;
            }

            return extension;
        }

        /// <summary>
        /// Returns the name and extension parts of the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string GetFileName(string path, string separator)
        {
            if(string.IsNullOrEmpty(path))
                return path;

            int nLast = path.LastIndexOf(separator);
            if(nLast >= 0)
            {
                return path.Substring(nLast + 1);
            }

            return path;
        }

        /// <summary>
        /// Returns the name and of the given path, minus the extension.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string GetFileNameWithoutExtension(string path, string separator)
        {
            return ChangeExtension(GetFileName(path, separator), null, separator);
        }

        /// <summary>
        /// Returns the extension of the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string GetExtension(string path, string separator)
        {
            if(path == null)
            {
                return null;
            }

            int iExt = findExtension(path, separator);

            if(iExt > -1)
            {
                if(iExt < path.Length - 1)
                {
                    return path.Substring(iExt);
                }
            }
            return string.Empty;
        }

        private static int findExtension(string path, string separator)
        {
            // method should return the index of the path extension
            // start or -1 if no valid extension
            if(path != null)
            {
                int iLastDot = path.LastIndexOf('.');
                int iLastSep = path.LastIndexOf(separator);

                if (iLastDot > iLastSep &&
                    !(path.Length == 0 && iLastDot == 0) && 
                    !(path.Length == 2 && iLastDot == 1 && path[0] == '.'))  // . and .. are not extensions
                {
                    return iLastDot;
                }
            }
            return -1;
        }


        // ReSharper restore InconsistentNaming
        // ReSharper restore RedundantIfElseBlock
        // ReSharper restore StringLastIndexOfIsCultureSpecific.1
        // ReSharper restore CSharpWarnings::CS1591
        #endregion
    }
}
