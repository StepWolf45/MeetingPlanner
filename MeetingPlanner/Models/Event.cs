using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeetingPlanner.Models
{
    public class CalendarEvent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        public int OrganizerId { get; set; }

        [ForeignKey("OrganizerId")]
        public virtual User Organizer { get; set; }

        public virtual ICollection<User> Attendees { get; set; }

        public virtual ICollection<EventInvitation> Invitations { get; set; }
    }
}