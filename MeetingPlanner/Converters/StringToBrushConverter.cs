using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MeetingPlanner.Converters
{
    public class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return Brushes.Transparent;

            try
            {
                return new BrushConverter().ConvertFromString(value.ToString()) as Brush;
            }
            catch
            {
                return Brushes.Gray; // Цвет по умолчанию при ошибке
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}