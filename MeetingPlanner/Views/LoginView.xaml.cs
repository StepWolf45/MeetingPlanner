using System.Windows;
using MeetingPlanner.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingPlanner.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<LoginViewModel>(); // Получение ViewModel из DI
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((LoginViewModel)DataContext).Password = ((PasswordBox)sender).Password;
            }
        }
    }
}