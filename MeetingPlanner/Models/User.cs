using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Media;

namespace MeetingPlanner.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        public string AvatarPath { get; set; }

        public bool HasPendingRequest { get; set; }

        [NotMapped] 
        public string FriendTag { get; set; }

        [NotMapped]
        public string FriendStatus { get; set; }

        [NotMapped]
        public Brush FriendStatusColor { get; set; }
        public virtual ICollection<FriendRequest> SentFriendRequests { get; set; }
        public virtual ICollection<FriendRequest> ReceivedFriendRequests { get; set; }
        public virtual ICollection<User> Friends { get; set; }
    }

    public class FriendRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        public int ReceiverId { get; set; }

        public bool IsAccepted { get; set; }

        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; }

        [ForeignKey("ReceiverId")]
        public virtual User Receiver { get; set; }

        public string FriendTag { get; set; }
    }
}