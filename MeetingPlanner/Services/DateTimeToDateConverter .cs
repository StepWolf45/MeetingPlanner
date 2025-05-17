using System;
using System.Globalization;
using System.Windows.Data;

namespace MeetingPlanner.Converters
{
    public class DateTimeToDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
                return date.Date;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
                return date.Date;
            return value;
        }
    }
}