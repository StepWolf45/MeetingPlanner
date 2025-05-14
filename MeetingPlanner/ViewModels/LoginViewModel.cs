using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingPlanner.Models;
using MeetingPlanner.Services;
using System.Threading.Tasks;
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
            set
            {
                SetProperty(ref _errorMessage, value);

                // Автоматическое скрытие через 3 секунды
                if (!string.IsNullOrEmpty(value))
                {
                    Task.Delay(3500).ContinueWith(_ =>
                    {
                        ErrorMessage = string.Empty;
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
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
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Поля логин и пароль обязательны для заполнения";
                return;
            }

            User user = _authenticationService.AuthenticateUser(Username, Password);

            if (user != null)
            {

                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.ShowHomeView(user);
            }
            else
            {
                ErrorMessage = "Неверный логин или пароль";
            }
        }
    }
}