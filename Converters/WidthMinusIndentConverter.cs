using System;
using System.Globalization;
using System.Windows.Data;

namespace TodoOverlayApp.Converters
{
    /// <summary>
    /// 将宽度减去缩进量的转换器，用于TreeView项目
    /// </summary>
    public class WidthMinusIndentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width)
            {
                // 参数是要减去的额外宽度（如滚动条宽度等）
                double extraWidth = 0;
                if (parameter is string paramStr && double.TryParse(paramStr, out double parsedWidth))
                {
                    extraWidth = parsedWidth;
                }

                // 返回调整后的宽度，确保不小于最小值
                return Math.Max(10, width - extraWidth);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
