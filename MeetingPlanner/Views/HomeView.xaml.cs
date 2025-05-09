using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MeetingPlanner.Models;
using MeetingPlanner.ViewModels;

namespace MeetingPlanner.Views
{
    public partial class HomeView : UserControl
    {
        public HomeView(HomeViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ChangeAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var viewModel = DataContext as HomeViewModel;
                viewModel.AvatarPath = openFileDialog.FileName;
                AvatarImage.Source = new BitmapImage(new Uri(viewModel.AvatarPath));
            }
        }
    }
}