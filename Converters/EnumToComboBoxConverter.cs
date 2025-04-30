using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace TodoOverlayApp.Converters
{
    /// <summary>
    /// 将枚举类型转换为ComboBox可绑定的数据源的转换器
    /// </summary>
    [ValueConversion(typeof(Enum), typeof(IEnumerable<KeyValuePair<Enum, string>>))]
    public class EnumToComboBoxConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 获取枚举类型
            Type enumType;
            if (value is Type valueType && valueType.IsEnum)
            {
                enumType = valueType;
            }
            else if (value is Enum)
            {
                enumType = value.GetType();
            }
            else
            {
                return new KeyValuePair<Enum, string>();
            }

            // 从枚举类型创建项列表
            var enumValues = Enum.GetValues(enumType).Cast<Enum>();
            var result = enumValues.Select(enumValue => new KeyValuePair<Enum, string>(
                enumValue,
                GetDescription(enumValue)
            )).ToList();

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is KeyValuePair<Enum, string> pair)
            {
                return pair.Key;
            }
            return null;
        }

        /// <summary>
        /// 获取枚举值的描述，优先从Description特性获取，否则使用美化的枚举名称
        /// </summary>
        private string GetDescription(Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null) return value.ToString();
            var attributes = (System.ComponentModel.DescriptionAttribute[])fieldInfo.GetCustomAttributes(
                typeof(System.ComponentModel.DescriptionAttribute), false);

            if (attributes.Length > 0)
            {
                return attributes[0].Description;
            }

            // 将枚举名称格式化为更易读的形式（例如"SomeValue"变为"Some Value"）
            return System.Text.RegularExpressions.Regex.Replace(value.ToString(), "([a-z])([A-Z])", "$1 $2");
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    /// <summary>
    /// 用于将枚举值转换为其显示名称的转换器
    /// </summary>
    public class EnumToDisplayNameConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum enumValue)
            {
                var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
                var attributes = (System.ComponentModel.DescriptionAttribute[])fieldInfo.GetCustomAttributes(
                    typeof(System.ComponentModel.DescriptionAttribute), false);

                if (attributes.Length > 0)
                {
                    return attributes[0].Description;
                }

                // 将枚举名称格式化为更易读的形式
                return System.Text.RegularExpressions.Regex.Replace(enumValue.ToString(), "([a-z])([A-Z])", "$1 $2");
            }

            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
            {
                // 尝试从显示名称转换回枚举值
                foreach (Enum enumValue in Enum.GetValues(targetType))
                {
                    if (this.Convert(enumValue, typeof(string), parameter, culture).ToString() == stringValue)
                    {
                        return enumValue;
                    }
                }

                // 尝试直接解析
                if (Enum.TryParse(targetType, stringValue, true, out object? result))
                {
                    return result;
                }
            }
            return Enum.GetValues(targetType).Cast<object>().FirstOrDefault();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
