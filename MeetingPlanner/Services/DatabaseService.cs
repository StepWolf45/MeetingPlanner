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
    }
}