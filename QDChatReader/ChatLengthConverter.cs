using System;
using System.Globalization;
using System.Windows.Data;

namespace QDChatReader
{
    [ValueConversion(typeof(long), typeof(string))]
    class ChatLengthConverter : IValueConverter
    {
        public static double DisplaySizeInch = 4.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] units = { "cm", "m", "km"};
            double size = 0;
            double.TryParse(value.ToString(), out size);
            double LinesToLengthFactor;
            if (DisplaySizeInch>0)
            {
                LinesToLengthFactor = DisplaySizeInch / 16.0;
            }
            else
            {
                LinesToLengthFactor = 0.25;
            }
            size = (double) size * LinesToLengthFactor;
            int unit = 0;

            if (size >= 100.0)
            {
                unit++;
                size /= 100.0;
            }

            if (size >= 1000.0)
            {
                unit++;
                size /= 1000.0;
            }

            return String.Format("{0:0.#} {1}", size, units[unit]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
