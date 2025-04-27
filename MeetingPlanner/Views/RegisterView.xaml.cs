using System.Windows;
using MeetingPlanner.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingPlanner.Views
{
    public partial class RegisterView : Window
    {
        public RegisterView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<RegisterViewModel>(); // Получение ViewModel из DI
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((RegisterViewModel)DataContext).Password = ((PasswordBox)sender).Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((RegisterViewModel)DataContext).ConfirmPassword = ((PasswordBox)sender).Password;
            }
        }
    }
}