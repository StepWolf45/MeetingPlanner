using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetingPlanner.Models;
using MeetingPlanner.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MeetingPlanner.ViewModels
{
    public class CalendarViewModel : ObservableObject
    {
        private readonly DatabaseService _db;
        private User _currentUser;
        private DateTime _selectedDate;
        private bool _isEventFormVisible;

        public ObservableCollection<CalendarEvent> Events { get; } = new ObservableCollection<CalendarEvent>();
        public ObservableCollection<User> Friends { get; } = new ObservableCollection<User>();
        public ObservableCollection<User> SelectedAttendees { get; } = new ObservableCollection<User>();

        public CalendarEvent NewEvent { get; } = new CalendarEvent();

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set => SetProperty(ref _selectedDate, value);
        }

        public bool IsEventFormVisible
        {
            get => _isEventFormVisible;
            set => SetProperty(ref _isEventFormVisible, value);
        }

        public ICommand CreateEventCommand { get; }
        public ICommand DateSelectedCommand { get; }
        public ICommand AddAttendeeCommand { get; }
        public ICommand RemoveAttendeeCommand { get; }

        public CalendarViewModel(DatabaseService db)
        {
            _db = db;

            CreateEventCommand = new RelayCommand(CreateEvent);
            DateSelectedCommand = new RelayCommand<DateTime?>(OnDateSelected);
            AddAttendeeCommand = new RelayCommand<User>(AddAttendee);
            RemoveAttendeeCommand = new RelayCommand<User>(RemoveAttendee);
        }

        public void SetCurrentUser(User currentUser)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _currentUser.Friends ??= new List<User>();
            LoadEvents();
            LoadFriends();
        }

        private void OnDateSelected(DateTime? date)
        {
            if (date.HasValue)
            {
                SelectedDate = date.Value;
                NewEvent.StartTime = date.Value;
                NewEvent.EndTime = date.Value.AddHours(1);
                IsEventFormVisible = true;
            }
            else
            {
                IsEventFormVisible = false;
            }
        }

        private void LoadEvents()
        {
            Events.Clear();
            var events = _db.CalendarEvents
                .Include(e => e.Organizer)
                .Include(e => e.Attendees)
                .Where(e => e.Organizer.Id == _currentUser.Id ||
                           e.Attendees.Any(a => a.Id == _currentUser.Id))
                .ToList();

            foreach (var ev in events)
            {
                Events.Add(ev);
            }
        }

        private void LoadFriends()
        {
            Friends.Clear();
            foreach (var friend in _currentUser.Friends.Where(f => f != null))
            {
                Friends.Add(friend);
            }
        }

        private void CreateEvent()
        {
            if (string.IsNullOrWhiteSpace(NewEvent.Title))
            {
                MessageBox.Show("Please enter event title", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NewEvent.Organizer = _currentUser;
            NewEvent.Attendees = SelectedAttendees.ToList();

            _db.CalendarEvents.Add(NewEvent);
            _db.SaveChanges();

            Events.Add(NewEvent);
            ResetEventForm();

            MessageBox.Show("Event created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ResetEventForm()
        {
            NewEvent.Title = string.Empty;
            NewEvent.Description = string.Empty;
            NewEvent.Location = string.Empty;
            NewEvent.StartTime = SelectedDate;
            NewEvent.EndTime = SelectedDate.AddHours(1);
            SelectedAttendees.Clear();
        }

        private void AddAttendee(User friend)
        {
            if (friend != null && !SelectedAttendees.Contains(friend))
            {
                SelectedAttendees.Add(friend);
            }
        }

        private void RemoveAttendee(User attendee)
        {
            SelectedAttendees.Remove(attendee);
        }
    }
}