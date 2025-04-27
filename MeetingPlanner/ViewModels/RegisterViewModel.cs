using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingPlanner.Services;
using System.Windows;

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
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(ConfirmPassword))
            {
                ErrorMessage = "All fields are required.";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return;
            }

            bool registrationSuccessful = _authenticationService.RegisterUser(Username, Password);

            if (registrationSuccessful)
            {
                MessageBox.Show("Registration successful!");
                // TODO: Navigate to login page
            }
            else
            {
                ErrorMessage = "Username already exists.";
            }
        }
    }
}