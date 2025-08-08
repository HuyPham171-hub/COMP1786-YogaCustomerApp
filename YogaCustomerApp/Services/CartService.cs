using System.Collections.ObjectModel;
using YogaCustomerApp.Models;

namespace YogaCustomerApp.Services
{
    public class CartService
    {
        private static CartService _instance;
        public static CartService Instance => _instance ??= new CartService();

        public ObservableCollection<CartItem> CartItems { get; set; } = new();

        private CartService() { }

        public void AddToCart(ClassInstance instance)
        {
            // Check if already in cart
            if (CartItems.Any(item => item.InstanceId == instance.Id))
            {
                App.Current.MainPage.DisplayAlert("Already in Cart", "This class is already in your cart.", "OK");
                return;
            }

            // Check cart limit
            if (CartItems.Count >= Constants.MaxCartItems)
            {
                App.Current.MainPage.DisplayAlert("Cart Full", $"You can only add up to {Constants.MaxCartItems} items to your cart.", "OK");
                return;
            }

            var cartItem = new CartItem
            {
                InstanceId = instance.Id,
                CourseId = instance.CourseId,
                Date = instance.Date,
                Teacher = instance.Teacher
            };

            CartItems.Add(cartItem);
            App.Current.MainPage.DisplayAlert("Added to Cart", "Class added to your cart successfully!", "OK");
        }

        public void RemoveFromCart(int instanceId)
        {
            var item = CartItems.FirstOrDefault(x => x.InstanceId == instanceId);
            if (item != null)
            {
                CartItems.Remove(item);
            }
        }

        public void ClearCart()
        {
            CartItems.Clear();
        }

        public List<int> GetInstanceIds()
        {
            return CartItems.Select(item => item.InstanceId).ToList();
        }

        public bool IsEmpty => CartItems.Count == 0;
    }
}
