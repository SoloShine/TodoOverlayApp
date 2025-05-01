using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using TodoOverlayApp.Utils;

namespace TodoOverlayApp.Converters
{
    /// <summary>
    /// 将应用程序路径转换为应用程序图标的转换器
    /// </summary>
    [ValueConversion(typeof(string), typeof(ImageSource))]
    public class AppPathToIconConverter : MarkupExtension, IValueConverter
    {
        // 默认图标缓存设置为true
        public bool UseCache { get; set; } = true;

        /// <summary>
        /// 将应用程序路径转换为应用程序图标
        /// </summary>
        /// <param name="value">应用程序路径</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">转换参数，可选：取值"nocache"表示不使用缓存</param>
        /// <param name="culture">区域信息</param>
        /// <returns>应用程序图标ImageSource</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string appPath)
            {
                try
                {
                    // 检查文件是否存在
                    if (!string.IsNullOrEmpty(appPath) && File.Exists(appPath))
                    {
                        // 检查是否使用缓存
                        bool useCache = UseCache;

                        // 如果参数是"nocache"，则不使用缓存
                        if (parameter is string paramStr && paramStr.Equals("nocache", StringComparison.OrdinalIgnoreCase))
                        {
                            useCache = false;
                        }

                        if (useCache)
                        {
                            // 使用缓存方式获取图标
                            string iconPath = IconHelper.GetCachedIconPath(appPath);
                            if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
                            {
                                var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                                bitmap.BeginInit();
                                bitmap.UriSource = new Uri(iconPath);
                                bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                                bitmap.EndInit();
                                return bitmap;
                            }
                        }

                        // 直接获取图标（不使用缓存或缓存失败时）
                        return IconHelper.GetIconFromPath(appPath);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"应用程序图标转换时出错: {ex.Message}");
                }
            }

            // 返回默认图标或null
            return null;
        }

        /// <summary>
        /// 反向转换（不支持）
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("无法将图标转换为应用程序路径");
        }

        /// <summary>
        /// 提供标记扩展的值
        /// </summary>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        /// <summary>
        /// 单例实例，方便在代码中使用
        /// </summary>
        private static AppPathToIconConverter _instance;
        public static AppPathToIconConverter Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AppPathToIconConverter();
                }
                return _instance;
            }
        }
    }
}
