using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using MeetingPlanner.Services;
using MeetingPlanner.ViewModels;
using MeetingPlanner.Views;

namespace MeetingPlanner
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
/*
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();*/
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
            services.AddSingleton<DatabaseService>();
            services.AddSingleton<AuthenticationService>();
            services.AddTransient<CalendarViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<ContactsViewModel>();
            services.AddTransient<RegisterViewModel>();
            services.AddTransient<LoginView>();
            services.AddTransient<HomeView>();
            services.AddTransient<ContactsView>();
            services.AddTransient<RegisterView>();
            services.AddTransient<CalendarView>();
        }
    }
}