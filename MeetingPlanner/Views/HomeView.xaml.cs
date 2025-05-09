using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MeetingPlanner.Models;
using MeetingPlanner.Services;

namespace MeetingPlanner.Views
{
    public partial class HomeView : UserControl
    {
        private readonly DatabaseService _db;
        private User _currentUser;

        public HomeView()
        {
            InitializeComponent();
            _db = new DatabaseService();
        }

        public void SetCurrentUser(User currentUser)
        {
            _currentUser = currentUser;
            LoadUserData();
        }

        private void LoadUserData()
        {
            if (_currentUser == null)
            {
                MessageBox.Show("Current user not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            FirstNameTextBox.Text = _currentUser.FirstName;
            LastNameTextBox.Text = _currentUser.LastName;
            if (!string.IsNullOrEmpty(_currentUser.AvatarPath))
            {
                AvatarImage.Source = new BitmapImage(new Uri(_currentUser.AvatarPath, UriKind.RelativeOrAbsolute));
            }
        }

        private void ChangeAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _currentUser.AvatarPath = openFileDialog.FileName;
                AvatarImage.Source = new BitmapImage(new Uri(_currentUser.AvatarPath));
            }
        }

        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            string firstName = FirstNameTextBox.Text;
            string lastName = LastNameTextBox.Text;

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                MessageBox.Show("First name and last name cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Обновите данные пользователя
            _currentUser.FirstName = firstName;
            _currentUser.LastName = lastName;

            // Сохраните изменения в базе данных
            _db.SaveChanges();

            // Сохраните аватарку, если это необходимо
            if (!string.IsNullOrEmpty(_currentUser.AvatarPath))
            {
                string destinationPath = Path.Combine("Avatars", Path.GetFileName(_currentUser.AvatarPath));
                Directory.CreateDirectory("Avatars");
                File.Copy(_currentUser.AvatarPath, destinationPath, true);
                _currentUser.AvatarPath = destinationPath;
                _db.SaveChanges();
            }

            MessageBox.Show("Changes saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}