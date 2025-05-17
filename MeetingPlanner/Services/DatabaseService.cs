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
        public DbSet<CalendarEvent> CalendarEvents { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CalendarEvent>()
                .HasRequired(e => e.Organizer)
                .WithMany()
                .WillCascadeOnDelete(false);

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