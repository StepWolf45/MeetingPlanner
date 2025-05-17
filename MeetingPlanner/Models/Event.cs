using System;
using System.Collections.Generic;

namespace MeetingPlanner.Models
{
    public class CalendarEvent
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; }
        public virtual ICollection<User> Attendees { get; set; } = new List<User>();
        public virtual User Organizer { get; set; }
    }
}