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
        private static ServiceProvider _serviceProvider;

        public static ServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Сервисы
            services.AddSingleton<DatabaseService>();
            services.AddSingleton<AuthenticationService>();

            // ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<RegisterViewModel>();

            // Views
            services.AddTransient<LoginView>();
            services.AddTransient<RegisterView>();
            services.AddTransient<HomeView>();
            services.AddSingleton<MainWindow>();
        }

/*        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }*/
    }
}