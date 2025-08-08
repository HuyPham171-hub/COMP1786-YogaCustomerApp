using System;
using System.Collections.ObjectModel;
using YogaCustomerApp.Models;
using YogaCustomerApp.Services;

namespace YogaCustomerApp.Pages
{
    public partial class BookingHistoryPage : ContentPage
    {
        public ObservableCollection<Booking> Bookings { get; set; } = new();
        private FirestoreService _firestoreService = new();

        public BookingHistoryPage()
        {
            InitializeComponent();
            BindingContext = this;
            bookingView.ItemsSource = Bookings;
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            try
            {
                string email = entryEmail.Text?.Trim();
                
                if (string.IsNullOrEmpty(email))
                {
                    await DisplayAlert("Error", "Please enter your email address.", "OK");
                    return;
                }

                if (!IsValidEmail(email))
                {
                    await DisplayAlert("Error", "Please enter a valid email address.", "OK");
                    return;
                }

                if (!NetworkUtil.IsConnected())
                {
                    await DisplayAlert("No Internet", "Please check your connection and try again.", "OK");
                    return;
                }

                statusLabel.Text = "Loading bookings...";
                
                var bookings = await _firestoreService.GetBookingsByEmailAsync(email);
                Bookings.Clear();
                
                foreach (var booking in bookings)
                {
                    Bookings.Add(booking);
                }

                statusLabel.Text = $"Found {Bookings.Count} booking(s) for {email}";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load bookings: {ex.Message}", "OK");
                statusLabel.Text = "Error loading bookings";
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var regex = new System.Text.RegularExpressions.Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }
    }
}
