using System.Collections.Generic;

namespace PathLib
{
    /// <summary>
    /// Represents the size of a particular file.
    /// </summary>
    public class FileSize
    {
        private readonly long _bytes;

        private readonly FileSizeScale _scale;

        private readonly Dictionary<FileSizeUnits, ConversionData> _conversionTable;

        /// <summary>
        /// Represents the size of a particular file.
        /// </summary>
        /// <param name="bytes"></param>
        public FileSize(long bytes)
            : this(bytes, FileSizeScale.Binary)
        {
        }

        /// <summary>
        /// Represents the size of a particular file.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="scale"></param>
        private FileSize(long bytes, FileSizeScale scale)
        {
            _bytes = bytes;
            _scale = scale;
            _conversionTable = scale == FileSizeScale.Binary
                ? BinaryConversionTable
                : SiConversionTable;
        }


        /// <summary>
        /// Returns the file size represented in multiples
        /// of 1000.
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public FileSize ToSIUnits()
        {
            return _scale == FileSizeScale.SI
                ? this
                : new FileSize(_bytes, FileSizeScale.SI);
        }

        public override bool Equals(object obj)
        {
            var other = obj as FileSize;
            if (other == null) return false;
            return _bytes == other._bytes;
        }

        public override int GetHashCode()
        {
            return _bytes.GetHashCode();
        }

        /// <summary>
        /// Size in bytes.
        /// </summary>
        public long Bytes
        {
            get { return _bytes; }
        }

        /// <summary>
        /// Size in bytes.
        /// </summary>
        public long Kilobytes
        {
            get { return GetAs(FileSizeUnits.Kilobyte); }
        }

        /// <summary>
        /// Size in bytes.
        /// </summary>
        public long Megabytes
        {
            get { return GetAs(FileSizeUnits.Megabyte); }
        }

        /// <summary>
        /// Size in bytes.
        /// </summary>
        public long Gigabytes
        {
            get { return GetAs(FileSizeUnits.Gigabyte); }
        }

        /// <summary>
        /// Size in bytes.
        /// </summary>
        public long Terabytes
        {
            get { return GetAs(FileSizeUnits.Terabyte); }
        }

        private long GetAs(FileSizeUnits units)
        {
            return _bytes/_conversionTable[units].Multiplier;
        }

        public override string ToString()
        {
            return _bytes + "B";
        }

        /// <summary>
        /// Returns the string using the given units.
        /// </summary>
        /// <param name="units"></param>
        /// <param name="useUnambiguousPrefixes">
        /// Use the new IEC prefixes for binary units (kibibytes-KiB,
        /// mebibytes-MiB, etc.) instead of the more common units.
        /// </param>
        /// <returns></returns>
        public string ToString(FileSizeUnits units, bool useUnambiguousPrefixes = false)
        {
            return GetAs(units) + (useUnambiguousPrefixes
                ? _conversionTable[units].UnambiguousPrefix
                : _conversionTable[units].BasicPrefix);
        }

        /// <summary>
        /// The scale used for prefixes.
        /// </summary>
        private enum FileSizeScale
        {
            /// <summary>
            /// Use the binary prefixes, where each
            /// prefix represents multiples of 1024.
            /// </summary>
            Binary = 0,

            /// <summary>
            /// Use the binary prefixes, where each
            /// prefix represents multiples of 1000.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            SI
        }

        private static readonly Dictionary<FileSizeUnits, ConversionData> BinaryConversionTable =
            new Dictionary<FileSizeUnits, ConversionData>
        {
            {FileSizeUnits.Byte, new ConversionData(1L, "B", "B")},
            {FileSizeUnits.Kilobyte, new ConversionData(1000L, "KB", "KiB")},
            {FileSizeUnits.Megabyte, new ConversionData(1000000L, "MB", "MiB")},
            {FileSizeUnits.Gigabyte, new ConversionData(1000000000L, "GB", "GiB")},
            {FileSizeUnits.Terabyte, new ConversionData(1000000000000L, "TB", "TiB")}
        };

        private static readonly Dictionary<FileSizeUnits, ConversionData> SiConversionTable = 
            new Dictionary<FileSizeUnits, ConversionData>
        {
            {FileSizeUnits.Byte, new ConversionData(1L, "B", "B")},
            {FileSizeUnits.Kilobyte, new ConversionData(1024L, "KB", "KB")},
            {FileSizeUnits.Megabyte, new ConversionData(1048576L, "MB", "MB")},
            {FileSizeUnits.Gigabyte, new ConversionData(1073741824L, "GB", "GB")},
            {FileSizeUnits.Terabyte, new ConversionData(1099511627776L, "TB", "TB")}
        };

        private class ConversionData
        {
            public long Multiplier { get; private set; }
            public string BasicPrefix { get; private set; }
            public string UnambiguousPrefix { get; private set; }

            public ConversionData(long mult, string basic, string unamb)
            {
                Multiplier = mult;
                BasicPrefix = basic;
                UnambiguousPrefix = unamb;
            }
        }
    }

    /// <summary>
    /// Units of file size.
    /// </summary>
    public enum FileSizeUnits
    {
        /// <summary>
        /// 
        /// </summary>
        Byte,
        /// <summary>
        /// 
        /// </summary>
        Kilobyte,
        /// <summary>
        /// 
        /// </summary>
        Megabyte,
        /// <summary>
        /// 
        /// </summary>
        Gigabyte,
        /// <summary>
        /// 
        /// </summary>
        Terabyte
    }
}
