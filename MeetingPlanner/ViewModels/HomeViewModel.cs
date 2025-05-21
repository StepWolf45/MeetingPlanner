using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingPlanner.Models;
using MeetingPlanner.Services;
using System.Windows;

namespace MeetingPlanner.ViewModels
{
    public class HomeViewModel : ObservableObject
    {
        private readonly DatabaseService _db;
        private User _currentUser;

        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set
            {
                SetProperty(ref _firstName, value);
                OnPropertyChanged(nameof(FullName));
            }
        }

        private string _lastName;
        public string LastName
        {
            get => _lastName;
            set
            {
                SetProperty(ref _lastName, value);
                OnPropertyChanged(nameof(FullName));
            }
        }

        private string _avatarPath;
        public string AvatarPath
        {
            get => _avatarPath;
            set => SetProperty(ref _avatarPath, value);
        }

        public string FullName => $"{FirstName} {LastName}";

        private CalendarViewModel _calendarViewModel;
        public CalendarViewModel CalendarViewModel
        {
            get => _calendarViewModel;
            set => SetProperty(ref _calendarViewModel, value);
        }
        public ContactsViewModel ContactsViewModel { get; }


        private InvitationsViewModel _invitationsViewModel;

        public InvitationsViewModel InvitationsViewModel
        {
            get => _invitationsViewModel;
            set => SetProperty(ref _invitationsViewModel, value);
        }
        public HomeViewModel(DatabaseService db, ContactsViewModel contactsViewModel, CalendarViewModel calendarViewModel, InvitationsViewModel invitationsViewModel)
        {
            _db = db;
            ContactsViewModel = contactsViewModel;
            CalendarViewModel = calendarViewModel;
            InvitationsViewModel = new InvitationsViewModel(db);
            SaveChangesCommand = new RelayCommand(SaveChanges);
            LogoutCommand = new RelayCommand(Logout);
        }

        public void SetCurrentUser(User currentUser)
        {
            _currentUser = currentUser;
            FirstName = _currentUser.FirstName;
            LastName = _currentUser.LastName;
            AvatarPath = _currentUser.AvatarPath;
            ContactsViewModel.SetCurrentUser(currentUser);
            CalendarViewModel.SetCurrentUser(currentUser);
            InvitationsViewModel.SetCurrentUser(currentUser);
        }

        public IRelayCommand SaveChangesCommand { get; }
        public IRelayCommand LogoutCommand { get; }

        private void SaveChanges()
        {
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            {
                MessageBox.Show("First name and last name cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _currentUser.FirstName = FirstName;
            _currentUser.LastName = LastName;
            _currentUser.AvatarPath = AvatarPath;

            _db.SaveChanges();

            MessageBox.Show("Changes saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Logout()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.ShowLoginView();
        }
    }
}