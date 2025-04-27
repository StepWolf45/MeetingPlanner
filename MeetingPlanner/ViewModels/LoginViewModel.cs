using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingPlanner.Models;
using MeetingPlanner.Services;
using System.Windows;

namespace MeetingPlanner.ViewModels
{
    readonly AuthenticationService _authenticationService;

    public LoginViewModel(AuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
        LoginCommand = new RelayCommand(Login);
    }

    public IRelayCommand LoginCommand { get; }

    private void Login()
    {
        User? user = _authenticationService.AuthenticateUser(Username, Password);

        if (user != null)
        {
            // Успешная аутентификация
            MessageBox.Show("Login successful!");
            //TODO: Перейти на другую страницу
        }
        else
        {
            ErrorMessage = "Invalid username or password.";
        }
    }
}
