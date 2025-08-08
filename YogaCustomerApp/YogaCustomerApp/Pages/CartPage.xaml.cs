using System.Text.RegularExpressions;
using YogaCustomerApp.Models;
using YogaCustomerApp.Services;

namespace YogaCustomerApp.Pages
{
    public partial class CartPage : ContentPage
    {
        public CartPage()
        {
            InitializeComponent();
            BindingContext = CartService.Instance;
            UpdateUI();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateUI();
        }

        private void UpdateUI()
        {
            var cartService = CartService.Instance;
            var hasItems = cartService.CartItems.Count > 0;
            
            cartView.IsVisible = hasItems;
            emptyCartLabel.IsVisible = !hasItems;
            
            UpdateEmailValidationLabel();
            UpdateSubmitButton();
        }

        private void OnEmailTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUI();
        }

        private void UpdateEmailValidationLabel()
        {
            var email = emailEntry.Text;
            if (string.IsNullOrWhiteSpace(email))
            {
                emailValidationLabel.IsVisible = false;
                return;
            }

            if (IsValidEmail(email))
            {
                emailValidationLabel.Text = "✓ Valid email format";
                emailValidationLabel.TextColor = Colors.Green;
                emailValidationLabel.IsVisible = true;
            }
            else
            {
                emailValidationLabel.Text = "✗ Please enter a valid email address";
                emailValidationLabel.TextColor = Colors.Red;
                emailValidationLabel.IsVisible = true;
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            try
            {
                var regex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                return regex.IsMatch(email);
            }
            catch { return false; }
        }

        private void UpdateSubmitButton()
        {
            var hasItems = CartService.Instance.CartItems.Count > 0;
            var hasValidEmail = IsValidEmail(emailEntry.Text);
            submitButton.IsEnabled = hasItems && hasValidEmail;
        }

        private void OnRemoveFromCartClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var cartItem = (CartItem)button.BindingContext;
            CartService.Instance.RemoveFromCart(cartItem.InstanceId);
            UpdateUI();
        }

        private void OnClearCartClicked(object sender, EventArgs e)
        {
            CartService.Instance.ClearCart();
            UpdateUI();
        }

        private async void OnSubmitBooking(object sender, EventArgs e)
        {
            var email = emailEntry.Text;
            if (!IsValidEmail(email))
            {
                await DisplayAlert("Error", "Please enter a valid email address", "OK");
                return;
            }

            if (!NetworkUtil.IsConnected())
            {
                await DisplayAlert("Error", "No internet connection. Please check your network and try again.", "OK");
                return;
            }

            var instanceIds = CartService.Instance.CartItems.Select(item => item.InstanceId).ToList();
            if (instanceIds.Count == 0)
            {
                await DisplayAlert("Error", "Cart is empty", "OK");
                return;
            }

            var firestoreService = new FirestoreService();
            await firestoreService.SubmitBooking(email, instanceIds);
            
            CartService.Instance.ClearCart();
            emailEntry.Text = "";
            UpdateUI();
        }
    }
}
