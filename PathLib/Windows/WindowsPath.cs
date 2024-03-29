﻿using PathLib.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable once CheckNamespace
namespace PathLib
{
    /// <summary>
    /// Concrete path implementation for Windows machines. Unusable on
    /// other systems.
    /// </summary>
    [TypeConverter(typeof(WindowsPathConverter))]
    public sealed class WindowsPath : ConcretePath<WindowsPath, PureWindowsPath>, IEquatable<WindowsPath>
    {
        private const string ExtendedLengthPrefix = @"\\?\";

        /// <summary>
        /// Create a new path object for Windows machines.
        /// </summary>
        /// <param name="paths"></param>
        public WindowsPath(params string[] paths)
            : base(new PureWindowsPath(paths))
        {
        }

        /// <summary>
        /// Create a new path object for Windows machines.
        /// </summary>
        /// <param name="path"></param>
        public WindowsPath(PureWindowsPath path)
            : base(path)
        {
        }

        /// <summary>
        /// TODO
        /// If a file name begins with only a disk designator 
        /// but not the backslash after the colon, it is 
        /// interpreted as a relative path to the current 
        /// directory on the drive with the specified letter. 
        /// Note that the current directory may or may not be 
        /// the root directory depending on what it was set to 
        /// during the most recent "change directory" operation 
        /// on that disk.
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa365247(v=vs.85).aspx
        /// </summary>
        private WindowsPath _cachedResolve;

        /// <inheritdoc/>
        public override WindowsPath Resolve()
        {
            if (_cachedResolve != null) return _cachedResolve;

            using (var fs = File.OpenRead(PurePath.ToString()))
            {
                if (fs.SafeFileHandle == null)
                {
                    return this;
                }
                var builder = new StringBuilder(512);
                GetFinalPathNameByHandle(fs.SafeFileHandle.DangerousGetHandle(),
                    builder, builder.Capacity, 0);
                var newPath = builder.ToString();

                if (newPath.StartsWith(ExtendedLengthPrefix) &&
                    !PurePath.ToString().StartsWith(ExtendedLengthPrefix))
                {
                    newPath = newPath.Substring(ExtendedLengthPrefix.Length);
                }
                return (_cachedResolve = new WindowsPath(newPath));
            }

        }

        /// <inheritdoc/>
        protected override WindowsPath PathFactory(params string[] paths)
        {
            if (paths == null)
            {
                throw new NullReferenceException(
                    "Path passed to factory cannot be null.");
            }
            return new WindowsPath(paths);
        }

        /// <inheritdoc/>
        protected override WindowsPath PathFactory(PureWindowsPath path)
        {
            if (path == null)
            {
                throw new NullReferenceException(
                    "Path passed to factory cannot be null.");
            }
            return new WindowsPath(path);
        }

        /// <inheritdoc/>
        protected override WindowsPath PathFactory(IPurePath path)
        {
            if (path == null)
            {
                throw new NullReferenceException(
                    "Path passed to factory cannot be null.");
            }
            var purePath = path as PureWindowsPath;
            if (purePath != null)
            {
                return new WindowsPath(purePath);
            }
            var parts = new List<string>();
            parts.AddRange(path.Parts);
            return new WindowsPath(parts.ToArray());
        }

        private StatInfo _cachedStat;

        /// <inheritdoc/>
        protected override StatInfo Stat(bool flushCache)
        {
            if(_cachedStat != null && !flushCache)
            {
                return _cachedStat;
            }

            // http://www.delorie.com/gnu/docs/glibc/libc_284.html
            var info = new FileInfo(PurePath.ToString());
            var stat = new StatInfo
            {
                Size = info.Length,
                ATime = info.LastAccessTimeUtc,
                MTime = info.LastWriteTimeUtc,
                CTime = info.CreationTimeUtc,
                Device = 0,
                Inode = 0,
                Gid = 0,
                Uid = 0,
                Mode = "0000" // TODO not implemented
            };

            _cachedStat = stat;
            return stat;
        }

        /// <inheritdoc/>
        public override bool IsSymlink()
        {
            ReparsePoint rep;
            return ReparsePoint.TryCreate(PurePath.ToString(), out rep) 
                && rep.Tag == ReparsePoint.TagType.SymbolicLink;
        }

        /// <summary>
        /// Return true if the path points to a junction. These are distinct
        /// from symlinks.
        /// </summary>
        /// <returns></returns>
        public bool IsJunction()
        {
            ReparsePoint rep;
            return ReparsePoint.TryCreate(PurePath.ToString(), out rep)
                   && rep.Tag == ReparsePoint.TagType.JunctionPoint;
        }

        /// <inheritdoc/>
        public override WindowsPath ExpandUser()
        {
            var homeDir = new PureWindowsPath("~");
            if (homeDir < PurePath)
            {
                var newDir = new PureWindowsPath(Environment.GetEnvironmentVariable("USERPROFILE"));
                return new WindowsPath(newDir.Join(PurePath.RelativeTo(homeDir)));
            }
            return this;
        }

        /// <summary>
        /// Return a path that uses the short "8.3" form of the
        /// filename. (e.g. DOCUME~2.docx) 
        /// </summary>
        /// <returns></returns>
        public WindowsPath ToShortPath()
        {
            var oldPath = PurePath.ToString();
            var newPath = new StringBuilder(255);
            if (GetShortPathName(oldPath, newPath, newPath.Capacity) == 0)
            {
                return this;
            }
            return new WindowsPath(PurePath.WithFilename(newPath.ToString()).ToString());
        }

        /// <summary>
        /// Convert a path that uses the short 8.3 name (e.g. DOCUME~2.docx)
        /// into its long path.
        /// </summary>
        /// <returns></returns>
        public WindowsPath ToLongPath()
        {
            var oldPath = PurePath.ToString();
            var newPath = new StringBuilder(255);
            if (GetLongPathName(oldPath, newPath, newPath.Capacity) == 0)
            {
                return this;
            }
            return new WindowsPath(PurePath.WithFilename(newPath.ToString()).ToString());
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetLongPathName(
            [MarshalAs(UnmanagedType.LPStr)] string path,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder longPath,
            int longPathLength);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetShortPathName(
            [MarshalAs(UnmanagedType.LPStr)] string path,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder shortPath,
            int longPathLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int GetFinalPathNameByHandle(
            IntPtr handle, [In, Out] StringBuilder path, int bufLen, int flags);


        #region Equality Members

        /// <summary>
        /// Compare two <see cref="PureWindowsPath"/> for equality.
        /// Case insensitive.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator ==(WindowsPath first, WindowsPath second)
        {
            return ReferenceEquals(first, null) ?
                ReferenceEquals(second, null) :
                first.Equals(second);
        }

        /// <summary>
        /// Compare two <see cref="PureWindowsPath"/> for inequality.
        /// Case insensitive.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator !=(WindowsPath first, WindowsPath second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Return true if <paramref name="first"/> is a parent path
        /// of <paramref name="second"/>.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator <(WindowsPath first, WindowsPath second)
        {
            return first.PurePath < second.PurePath;
        }

        /// <summary>
        /// Return true if <paramref name="second"/> is a parent of
        /// <paramref name="first"/>.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator >(WindowsPath first, WindowsPath second)
        {
            return second < first;
        }

        /// <summary>
        /// Return true if <paramref name="first"/> is equal to or a parent
        /// of <paramref name="second"/>.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator <=(WindowsPath first, WindowsPath second)
        {
            return first.PurePath <= second.PurePath;
        }

        /// <summary>
        /// Return true if <paramref name="second"/> is equal to or a parent
        /// of <paramref name="first"/>.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator >=(WindowsPath first, WindowsPath second)
        {
            return first.PurePath >= second.PurePath;
        }

        /// <summary>
        /// Compare two <see cref="PureWindowsPath"/> for equality.
        /// Case insensitive.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(WindowsPath other)
        {
            if (other is null) return false;
            return PurePath.Equals(other.PurePath);
        }

        /// <summary>
        /// Compare two <see cref="PureWindowsPath"/> for equality.
        /// Case insensitive.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(object other)
        {
            var obj = other as WindowsPath;
            return !ReferenceEquals(obj, null) && Equals(obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return PurePath.GetHashCode();
        }

        #endregion

        /// <inheritdoc/>
        public override string ToString()
        {
            return PurePath.ToString();
        }

        /// <inheritdoc/>
        public override IEnumerable<DirectoryContents<WindowsPath>> WalkDir(Action<IOException> onError = null)
        {
            var subdirs = new Queue<WindowsPath>();
            subdirs.Enqueue(this);

            IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
            FindFileNative.WIN32_FIND_DATA findData;

            // Breadth-first search
            while (subdirs.Count > 0)
            {
                var directory = subdirs.Dequeue();

                // please note that the following line won't work if you try this on a network folder, like \\Machine\C$
                // simply remove the \\?\ part in this case or use \\?\UNC\ prefix
                using (FindFileNative.SafeFindHandle findHandle = FindFileNative.FindFirstFile(@"\\?\" + directory + @"\*", out findData))
                {
                    if (!findHandle.IsInvalid)
                    {
                        var content = new DirectoryContents<WindowsPath>(directory);
                        do
                        {
                            var entry = PathFactory(findData.cFileName);
                            if ((findData.dwFileAttributes & FileAttributes.Directory) != 0)
                            {

                                if (findData.cFileName == "." || findData.cFileName == "..")
                                {
                                    continue;  // skip self and parent
                                }
                                content.Directories.Add(entry);
                            }
                            else
                            {
                                //files++;
                                content.Files.Add(entry);
                            }
                        }
                        while (FindFileNative.FindNextFile(findHandle, out findData));

                        yield return content;
                        foreach (var entry in content.Directories)
                        {
                            if (entry != null)
                            {
                                subdirs.Enqueue(directory.Join(entry));
                            }
                        }
                    }
                    else
                    {
                        if (onError != null)
                        {
                            var err = new Win32Exception();
                            onError(new IOException(String.Format("Unable to enter directory {0}. \r\n{1}", directory, err.Message)));
                        }
                    }
                }
            }
        }
    }
}
