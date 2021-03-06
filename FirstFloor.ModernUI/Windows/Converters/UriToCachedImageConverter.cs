﻿using System;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media.Imaging;
using FirstFloor.ModernUI.Helpers;
using FirstFloor.ModernUI.Serialization;
using FirstFloor.ModernUI.Windows.Controls;

namespace FirstFloor.ModernUI.Windows.Converters {
    [ValueConversion(typeof(object), typeof(BitmapSource))]
    public class UriToCachedImageConverter : IValueConverter {
        public const double OneTrueDpi = 96d;

        public static BitmapSource ConvertBitmapToOneTrueDpi(BitmapImage bitmapImage) {
            var width = bitmapImage.PixelWidth;
            var height = bitmapImage.PixelHeight;

            var stride = width * bitmapImage.Format.BitsPerPixel;
            var pixelData = new byte[stride * height];
            bitmapImage.CopyPixels(pixelData, stride, 0);

            return BitmapSource.Create(width, height, OneTrueDpi, OneTrueDpi, bitmapImage.Format, null, pixelData, stride);
        }

        public static BitmapSource Convert(object value, bool considerOneTrueDpi = false, int decodeWidth = -1, int decodeHeight = -1) {
            if (value is BitmapSource) return value as BitmapImage;

            var source = value as Uri;
            if (source == null) {
                var path = value?.ToString();
                if (string.IsNullOrEmpty(path)) {
                    return null;
                }

                try {
                    source = new Uri(path);
                } catch (Exception) {
                    Logging.Warning("Invalid URI format: " + path);
                    return null;
                }
            }

            try {
                var bi = new BitmapImage();
                bi.BeginInit();

                if (decodeWidth != -1) {
                    bi.DecodePixelWidth = (int)(decodeWidth * Math.Max(DpiInformation.MaxScaleX, 1d));
                }

                if (decodeHeight != -1) {
                    bi.DecodePixelHeight = (int)(decodeHeight * Math.Max(DpiInformation.MaxScaleY, 1d));
                }

                bi.CreateOptions = source.Scheme == "http" || source.Scheme == "https"
                        ? BitmapCreateOptions.None : BitmapCreateOptions.IgnoreImageCache;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.UriSource = source;
                bi.EndInit();
                bi.Freeze();

                if (considerOneTrueDpi && (Math.Abs(bi.DpiX - OneTrueDpi) > 1 || Math.Abs(bi.DpiY - OneTrueDpi) > 1)) {
                    return ConvertBitmapToOneTrueDpi(bi);
                }

                return bi;
            } catch (Exception) {
                return null;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (parameter is string p) {
                var i = p.IndexOf('×');
                return i != -1
                        ? Convert(value, false, i == 0 ? -1 : p.Substring(0, i).As<int>(), i == p.Length - 1 ? -1 : p.Substring(i + 1).As<int>())
                        : Convert(value, p == "oneTrueDpi");
            }

            return Convert(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
