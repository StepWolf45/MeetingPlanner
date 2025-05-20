using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MeetingPlanner.Converters
{
    public class FriendStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string status && status == "Добавить в друзья"
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}