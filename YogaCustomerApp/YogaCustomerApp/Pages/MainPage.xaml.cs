using Microsoft.Maui.Controls;
using YogaCustomerApp.Services;

namespace YogaCustomerApp.Pages
{
    public partial class MainPage : ContentPage
    {
        private CartService _cartService = CartService.Instance;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            int cartItems = _cartService.CartItems.Count;
            if (cartItems > 0)
            {
                statusLabel.Text = $"You have {cartItems} item(s) in your cart";
            }
            else
            {
                statusLabel.Text = "Welcome to Universal Yoga!";
            }
        }

        private async void OnViewClassesClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ClassListPage());
        }

        private async void OnViewCartClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CartPage());
        }

        private async void OnViewHistoryClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new BookingHistoryPage());
        }
    }
}
