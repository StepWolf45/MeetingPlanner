using System.Data.Entity;
using MeetingPlanner.Models;

namespace MeetingPlanner.Services
{
    public class DatabaseService : DbContext
    {
        public DatabaseService() : base("name=MeetingPlannerConnectionString")
        {
            //Database.SetInitializer(new CreateDatabaseIfNotExists<DatabaseService>()); //Создает БД, если ее нет
            //Если хотите миграции, раскомментируйте следующую строку:
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<MeetingPlanner.Services.DatabaseService, Migrations.Configuration>());
        }

        public DbSet<User> Users { get; set; }
    }
}