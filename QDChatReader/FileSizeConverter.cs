using System;
using System.Globalization;
using System.Windows.Data;

namespace QDChatReader
{
    [ValueConversion(typeof(long), typeof(string))]
    class FileSizeConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            double size = 0;
            double.TryParse((string)value, out size);
            int unit = 0;

            while (size >= 1024)
            {
                size /= 1024;
                ++unit;
            }

            return String.Format("{0:0.#} {1}", size, units[unit]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
