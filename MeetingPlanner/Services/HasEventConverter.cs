using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using MeetingPlanner.Models;

namespace MeetingPlanner.Converters
{
    public class HasEventConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && 
                values[0] is ObservableCollection<CalendarEvent> events &&
                values[1] is DateTime date)
            {
                return events.Any(e => e.StartTime.Date <= date.Date && e.EndTime.Date >= date.Date);
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}