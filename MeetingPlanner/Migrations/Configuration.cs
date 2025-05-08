using System.Data.Entity.Migrations;
using System.Data.SQLite.EF6.Migrations; // Добавьте эту строку

namespace MeetingPlanner.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<MeetingPlanner.Services.DatabaseService>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            SetSqlGenerator("System.Data.SQLite", new SQLiteMigrationSqlGenerator()); // Добавьте эту строку
        }

        protected override void Seed(MeetingPlanner.Services.DatabaseService context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
        }
    }
}