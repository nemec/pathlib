using System;

namespace PathLib
{
    /// <summary>
    /// File attributes.
    /// </summary>
    public sealed class StatInfo
    {
        /// <summary>
        /// protection bits
        /// st_mode
        /// </summary>
        public uint ModeDecimal { get; set; }
        
        /// <summary>
        /// st_mode Converted into an octal string
        /// </summary>
        public string Mode { get; set; }

        /// <summary>
        /// inode number
        /// st_ino
        /// </summary>
        public long Inode { get; set; }

        /// <summary>
        /// st_dev
        /// </summary>
        public long Device { get; set; }

        /// <summary>
        /// st_nlink
        /// </summary>
        public long NumLinks { get; set; }

        /// <summary>
        /// st_uid
        /// </summary>
        public long Uid { get; set; }

        /// <summary>
        /// st_gid
        /// </summary>
        public long Gid { get; set; }

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
