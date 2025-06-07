using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingPlanner.Models;
using MeetingPlanner.Services;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MeetingPlanner.ViewModels
{
    public class LoginViewModel : ObservableObject
    {
        private string _username;
        private string _password;
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

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                // Отменяем предыдущий таймер
                _errorMessageCts?.Cancel();

                // Устанавливаем новое сообщение
                SetProperty(ref _errorMessage, value);

                // Если сообщение не пустое - запускаем таймер
                if (!string.IsNullOrEmpty(value))
                {
                    _errorMessageCts = new CancellationTokenSource();
                    var token = _errorMessageCts.Token;

                    Task.Delay(3500, token).ContinueWith(t =>
                    {
                        // Если таймер не был отменен - очищаем сообщение
                        if (!t.IsCanceled && !token.IsCancellationRequested)
                        {
                            ErrorMessage = string.Empty;
                        }
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