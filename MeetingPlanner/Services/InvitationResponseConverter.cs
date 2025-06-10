using MeetingPlanner.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MeetingPlanner.Converters
{
    public class InvitationResponseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EventInvitation invitation && parameter is string tag)
            {
                return new InvitationResponse { Invitation = invitation, Tag = tag };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InvitationResponse
    {
        public EventInvitation Invitation { get; set; }
        public string Tag { get; set; }
    }
}