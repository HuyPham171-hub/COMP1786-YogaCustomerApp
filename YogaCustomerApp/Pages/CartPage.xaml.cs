using System.Text.RegularExpressions;
using YogaCustomerApp.Models;
using YogaCustomerApp.Services;

namespace YogaCustomerApp.Pages
{
    public partial class CartPage : ContentPage
    {
        private CartService _cartService = CartService.Instance;
        private FirestoreService _firestoreService = new();

        public CartPage()
        {
            InitializeComponent();
            BindingContext = _cartService;
            cartView.ItemsSource = _cartService.CartItems;
            
            // Subscribe to cart changes
            _cartService.CartItems.CollectionChanged += CartItems_CollectionChanged;
            
            UpdateUI();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateUI();
        }

        private void CartItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            bool hasItems = _cartService.CartItems.Count > 0;
            emptyCartLabel.IsVisible = !hasItems;
            cartView.IsVisible = hasItems;
            
            // Enable submit button only if cart has items and email is valid
            bool hasValidEmail = IsValidEmail(emailEntry.Text);
            submitButton.IsEnabled = hasItems && hasValidEmail;
        }

        private void OnRemoveFromCartClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is CartItem item)
            {
                _cartService.RemoveFromCart(item.InstanceId);
            }
        }

        private void OnClearCartClicked(object sender, EventArgs e)
        {
            _cartService.ClearCart();
            emailEntry.Text = "";
            UpdateUI();
        }

        private async void OnSubmitBooking(object sender, EventArgs e)
        {
            try
            {
                string email = emailEntry.Text?.Trim();
                
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

                if (_cartService.IsEmpty)
                {
                    await DisplayAlert("Error", "Your cart is empty.", "OK");
                    return;
                }

                if (!NetworkUtil.IsConnected())
                {
                    await DisplayAlert("No Internet", "Please check your connection and try again.", "OK");
                    return;
                }

                // Show confirmation dialog
                bool confirmed = await DisplayAlert("Confirm Booking", 
                    $"Submit booking for {_cartService.CartItems.Count} class(es) to {email}?", 
                    "Yes", "No");

                if (!confirmed) return;

                // Submit booking
                var instanceIds = _cartService.GetInstanceIds();
                await _firestoreService.SubmitBooking(email, instanceIds);

                // Clear cart after successful booking
                _cartService.ClearCart();
                emailEntry.Text = "";
                UpdateUI();

                await DisplayAlert("Success", "Your booking has been submitted successfully!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to submit booking: {ex.Message}", "OK");
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private void OnEmailTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUI();
        }
    }
}
