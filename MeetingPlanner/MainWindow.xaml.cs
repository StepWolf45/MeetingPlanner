using System.ComponentModel;
using System.Windows;
using MeetingPlanner.ViewModels;
using MeetingPlanner.Views;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingPlanner
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Content = App.ServiceProvider.GetRequiredService<LoginView>(); // Отображаем LoginView при запуске
        }
    }
}