using System.Windows;
using System.Windows.Controls; // Добавьте эту строку
using MeetingPlanner.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingPlanner.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<LoginViewModel>(); 
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((LoginViewModel)DataContext).Password = ((PasswordBox)sender).Password;
            }
        }

        private void GoToRegister_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.ShowRegisterView();
        }
    }
}