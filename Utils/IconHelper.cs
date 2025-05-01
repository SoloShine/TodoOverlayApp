using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TodoOverlayApp.Utils
{
    /// <summary>
    /// 图标辅助类，用于从文件获取程序图标
    /// </summary>
    public static class IconHelper
    {
        /// <summary>
        /// 从程序路径获取图标
        /// </summary>
        /// <param name="path">程序路径</param>
        /// <returns>图标的ImageSource</returns>
        public static ImageSource GetIconFromPath(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return null;

            try
            {
                // 使用 System.Drawing 获取图标
                using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(path))
                {
                    if (icon != null)
                    {
                        // 将 System.Drawing.Icon 转换为 ImageSource
                        return Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取图标时出错: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 从程序路径获取图标并保存到指定路径
        /// </summary>
        /// <param name="path">程序路径</param>
        /// <param name="savePath">保存路径</param>
        /// <returns>是否保存成功</returns>
        public static bool SaveIconFromPath(string path, string savePath)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return false;

            try
            {
                ImageSource imageSource = GetIconFromPath(path);
                if (imageSource is BitmapSource bitmapSource)
                {
                    // 创建文件目录
                    string directory = Path.GetDirectoryName(savePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    // 保存为PNG图片
                    using (var fileStream = new FileStream(savePath, FileMode.Create))
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                        encoder.Save(fileStream);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存图标时出错: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// 从程序路径获取并缓存图标
        /// </summary>
        /// <param name="path">程序路径</param>
        /// <returns>图标的本地缓存路径</returns>
        public static string GetCachedIconPath(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return null;

            try
            {
                string fileName = Path.GetFileNameWithoutExtension(path);
                string fileHash = BitConverter.ToString(
                    System.Security.Cryptography.MD5.Create().ComputeHash(
                        System.Text.Encoding.UTF8.GetBytes(path))).Replace("-", "");

                // 创建缓存目录
                string cacheDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "TodoOverlayApp", "IconCache");

                if (!Directory.Exists(cacheDir))
                    Directory.CreateDirectory(cacheDir);

                // 缓存文件路径
                string iconPath = Path.Combine(cacheDir, $"{fileName}_{fileHash}.png");

                // 如果缓存文件已存在则直接返回
                if (File.Exists(iconPath))
                    return iconPath;

                // 保存新的图标缓存
                if (SaveIconFromPath(path, iconPath))
                    return iconPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"缓存图标时出错: {ex.Message}");
            }

            return null;
        }
    }
}
