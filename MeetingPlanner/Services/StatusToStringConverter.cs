using System;
using System.Globalization;
using System.Windows.Data;
using MeetingPlanner.Models;

namespace MeetingPlanner.Converters
{
    public class StatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ResponseStatus status))
            {
                return "Неизвестный статус";
            }

            switch (status)
            {
                case ResponseStatus.Accepted:
                    return "Приду";
                case ResponseStatus.Declined:
                    return "Не приду";
                case ResponseStatus.Maybe:
                    return "Возможно";
                case ResponseStatus.Pending:
                    return "Еще не ответили";
                default:
                    return "Неизвестный статус";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}