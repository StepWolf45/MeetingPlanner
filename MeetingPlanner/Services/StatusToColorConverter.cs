using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MeetingPlanner.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Brushes.LightGray;

            var status = value.ToString();
            switch (status)
            {
                case "Придет":
                case "Организатор":
                    return Brushes.LightGreen;
                case "Не придет":
                    return Brushes.LightPink;
                case "Возможно":
                    return Brushes.LightYellow;
                case "Не отвечено":
                    return Brushes.LightGray;
                default:
                    return Brushes.LightBlue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}