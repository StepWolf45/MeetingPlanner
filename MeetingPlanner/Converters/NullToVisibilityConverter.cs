using MeetingPlanner.Models;
using MeetingPlanner.ViewModels;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MeetingPlanner.Converters
{
    public class TagVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is ContactsViewModel vm && values[1] is User friend)
            {
                var tag = vm.GetFriendTag(friend);
                return tag?.TagName != null ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FriendTagToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Func<User, FriendTag> getTagFunc && parameter is User friend)
            {
                return getTagFunc(friend)?.TagName ?? string.Empty;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class FriendTagConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is ContactsViewModel vm && values[1] is User friend)
            {
                var tag = vm.GetFriendTag(friend);
                return new
                {
                    TagName = tag?.TagName,
                    TagColor = tag?.TagColor
                };
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}