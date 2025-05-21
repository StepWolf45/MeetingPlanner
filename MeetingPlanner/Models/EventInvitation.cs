
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeetingPlanner.Models
{
    public class EventInvitation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int UserId { get; set; }

        public ResponseStatus Status { get; set; } = ResponseStatus.Pending;
        public string CustomResponse { get; set; }

        [ForeignKey("EventId")]
        public virtual CalendarEvent Event { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }

    public enum ResponseStatus
    {
        Pending,
        Accepted,
        Declined,
        Maybe,
        Custom
    }
}