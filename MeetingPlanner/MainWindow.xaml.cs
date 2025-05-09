using System.Windows;
using MeetingPlanner.Views;
using MeetingPlanner.Models;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingPlanner
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ShowLoginView();
        }

        public void ShowLoginView()
        {
            MainContentControl.Content = App.ServiceProvider.GetRequiredService<LoginView>();
        }

        public void ShowRegisterView()
        {
            MainContentControl.Content = App.ServiceProvider.GetRequiredService<RegisterView>();
        }

        public void ShowHomeView(User currentUser)
        {
            var homeView = App.ServiceProvider.GetRequiredService<HomeView>();
            homeView.SetCurrentUser(currentUser);
            MainContentControl.Content = homeView;
        }
    }
}