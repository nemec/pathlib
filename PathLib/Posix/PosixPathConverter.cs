using System;
using System.ComponentModel;
using System.Globalization;

namespace PathLib
{
    /// <summary>
    /// Turn a string into a Windows path.
    /// </summary>
    public class PosixPathConverter : TypeConverter
    {
        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc/>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var path = value as string;
            if (path != null)
            {
                return new PosixPath(path);
            }
            return base.ConvertFrom(context, culture, value);
        }

        /// <inheritdoc/>
        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            if (value is string)
            {
                PurePosixPath path;
                return PurePosixPath.TryParse(value as string, out path);
            }
            return base.IsValid(context, value);
        }
    }
}
