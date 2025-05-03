
using System;
using System.Globalization;
using System.Windows.Data;

namespace TodoOverlayApp.Converters
{
    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is DateTime))
                return string.Empty;

            DateTime dateTime = (DateTime)value;
            DateTime now = DateTime.Now;
            TimeSpan difference = dateTime - now;

            // 同一天只显示时间
            if (dateTime.Date == now.Date)
                return dateTime.ToString("HH:mm");

            // 相差不超过3天
            if (Math.Abs(difference.TotalDays) <= 3)
            {
                if (difference.TotalDays >= -1 && difference.TotalDays < 0)
                    return $"昨天{dateTime.ToString("HH:mm")}";
                if (difference.TotalDays == -1)
                    return $"前天{dateTime.ToString("HH:mm")}";
                if (difference.TotalDays == 1)
                    return $"明天{dateTime.ToString("HH:mm")}";
                if (difference.TotalDays == 2)
                    return $"后天{dateTime.ToString("HH:mm")}";
            }

            // 同一年且不超过1周
            if (dateTime.Year == now.Year && Math.Abs(difference.TotalDays) <= 7)
                return dateTime.ToString("MM/dd");

            // 超过1年或跨年
            return dateTime.ToString("yyyy/MM/dd");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
