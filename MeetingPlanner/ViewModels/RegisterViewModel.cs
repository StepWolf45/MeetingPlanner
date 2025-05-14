using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingPlanner.Services;
using System.Windows;
using System.Linq;

namespace MeetingPlanner.ViewModels
{
    public class RegisterViewModel : ObservableObject
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

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private readonly AuthenticationService _authenticationService;

        public RegisterViewModel(AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
            RegisterCommand = new RelayCommand(Register);
        }

        public IRelayCommand RegisterCommand { get; }

        private void Register()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ErrorMessage = "All fields are required.";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return;
            }

            if (Password.Length < 5 || Password.Length > 10)
            {
                ErrorMessage = "Password must be between 5 and 10 characters.";
                return;
            }

            if (!Password.Any(char.IsDigit) || !Password.Any(char.IsLetter))
            {
                ErrorMessage = "Password must contain both letters and numbers.";
                return;
            }

            bool registrationSuccessful = _authenticationService.RegisterUser(Username, Password);

            if (registrationSuccessful)
            {
                MessageBox.Show("Registration successful!");
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.ShowLoginView();
            }
            else
            {
                ErrorMessage = "Username already exists.";
            }
        }
    }
}