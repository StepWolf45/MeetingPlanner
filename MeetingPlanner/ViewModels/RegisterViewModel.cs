using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingPlanner.Services;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MeetingPlanner.ViewModels
{
    public class RegisterViewModel : ObservableObject
    {
        private string _username;
        private string _password;
        private string _confirmPassword;
        private string _errorMessage;
        private CancellationTokenSource _errorMessageCts;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessageCts?.Cancel();
                SetProperty(ref _errorMessage, value);
                if (!string.IsNullOrEmpty(value))
                {
                    _errorMessageCts = new CancellationTokenSource();
                    var token = _errorMessageCts.Token;

                    Task.Delay(3500, token).ContinueWith(t =>
                    {
                        if (!t.IsCanceled && !token.IsCancellationRequested)
                        {
                            ErrorMessage = string.Empty;
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
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
                ErrorMessage = "Необходимо заполнить все поля";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Пароли не совпадают";
                return;
            }

            if (Password.Length < 5 || Password.Length > 10)
            {
                ErrorMessage = "Пароль должен находится в диапозоне от 5 до 10 символов";
                return;
            }

            if (!Password.Any(char.IsDigit) || !Password.Any(char.IsLetter))
            {
                ErrorMessage = "Пароль должен содержать цифры и буквы";
                return;
            }

            bool registrationSuccessful = _authenticationService.RegisterUser(Username, Password);

            if (registrationSuccessful)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.ShowLoginView();
            }
            else
            {
                ErrorMessage = "Такой логин уже существует";
            }
        }
    }
}