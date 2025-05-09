using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingPlanner.Models;
using MeetingPlanner.Services;
using System.Windows;

namespace MeetingPlanner.ViewModels
{
    public class LoginViewModel : ObservableObject
    {
        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private readonly AuthenticationService _authenticationService;

        public LoginViewModel(AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
            LoginCommand = new RelayCommand(Login);
        }

        public IRelayCommand LoginCommand { get; }

        private void Login()
        {
            User user = _authenticationService.AuthenticateUser(Username, Password);

            if (user != null)
            {
                // Успешная аутентификация
                MessageBox.Show("Login successful!");
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.ShowHomeView(user);
            }
            else
            {
                ErrorMessage = "Invalid username or password.";
            }
        }
    }
}