using System;
using System.Globalization;
using System.Windows.Data;

namespace MeetingPlanner.Converters
{
    public class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
                return timeSpan.ToString(@"hh\:mm");
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (TimeSpan.TryParse(value as string, out var result))
                return result;
            return value;
        }
    }
}