using System;
using System.ComponentModel;

namespace PathLib.Converters
{
    /// <summary>
    /// Adds type conversion support from strings to paths.
    /// </summary>
    public class PureNtPathConverter : TypeConverter
    {
        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof (string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc/>
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            var path = value as string;
            if (path != null)
            {
                return new PureNtPath(path);
            }
            return base.ConvertFrom(context, culture, value);
        }

        /// <inheritdoc/>
        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            if (value is string)
            {
				PureNtPath path;
				return PureNtPath.TryParse(value as string, out path);
            }
            return base.IsValid(context, value);
        }
    }
}
