using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EAAddInBase.Utils
{
    public sealed class OptionConverter : TypeConverter
    {
        private readonly Type innerType;

        private readonly TypeConverter innerTypeConverter;

        public OptionConverter(Type type)
        {
            innerType = type.GetGenericArguments()[0];
            innerTypeConverter = TypeDescriptor.GetConverter(innerType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(String))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is String)
            {
                return FromString(context, culture, value as String);
            }
            return base.ConvertFrom(context, culture, value);
        }

        private object FromString(ITypeDescriptorContext context, CultureInfo culture, String value)
        {
            var someMatch = Regex.Match(value, @"Some\((.*)\)");

            return (value.Equals("None") || someMatch.Success)
                .Then(() =>
                {
                    var ctorParams = someMatch.Success
                        .Then(() =>
                        {
                            var innerValue = innerTypeConverter.ConvertFrom(context, culture, someMatch.Groups[1].Value);
                            return new object[] { innerValue };
                        })
                        .Else(() => new object[] { });

                    var parameterizedOptionType = typeof(Option<>).MakeGenericType(innerType);

                    return Activator.CreateInstance(
                        parameterizedOptionType, BindingFlags.NonPublic | BindingFlags.Instance, null, ctorParams, CultureInfo.InvariantCulture);
                })
                .Else(() => base.ConvertFromString(context, culture, value));
        }
    }
}
