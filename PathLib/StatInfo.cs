using System;

namespace PathLib
{
    /// <summary>
    /// File attributes.
    /// </summary>
    public class StatInfo
    {
        /// <summary>
        /// protection bits
        /// st_mode
        /// </summary>
        public int Mode { get; set; }

        /// <summary>
        /// inode number
        /// st_ino
        /// </summary>
        public long Inode { get; set; }

        /// <summary>
        /// st_dev
        /// </summary>
        public int Device { get; set; }

        /// <summary>
        /// st_nlink
        /// </summary>
        public int NumLinks { get; set; }

        /// <summary>
        /// st_uid
        /// </summary>
        public int Uid { get; set; }

        /// <summary>
        /// st_gid
        /// </summary>
        public int Gid { get; set; }

        /// <summary>
        /// st_size
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// st_atime
        /// </summary>
        public DateTime ATime { get; set; }

        /// <summary>
        /// st_mtime
        /// </summary>
        public DateTime MTime { get; set; }

        /// <summary>
        /// st_ctime
        /// </summary>
        public DateTime CTime { get; set; }
    }
}
