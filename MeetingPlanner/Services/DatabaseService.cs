using System.Data.Entity;
using MeetingPlanner.Models;

namespace MeetingPlanner.Services
{
    public class DatabaseService : DbContext
    {
        public DatabaseService() : base("name=MeetingPlannerConnectionString")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<DatabaseService, Migrations.Configuration>());
        }

        public DbSet<User> Users { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<FriendTag> FriendTags { get; set; }
        public DbSet<CalendarEvent> CalendarEvents { get; set; }

        public DbSet<EventInvitation> EventInvitations { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Friends)
                .WithMany()
                .Map(m =>
                {
                    m.ToTable("UserFriends");
                    m.MapLeftKey("UserId");
                    m.MapRightKey("FriendId");
                });

            modelBuilder.Entity<CalendarEvent>()
                .HasMany(e => e.Attendees)
                .WithMany()
                .Map(m =>
                {
                    m.ToTable("EventAttendees");
                    m.MapLeftKey("EventId");
                    m.MapRightKey("UserId");
                });
        }
    }
}