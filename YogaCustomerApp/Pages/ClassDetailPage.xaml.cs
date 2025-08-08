using YogaCustomerApp.Models;
using YogaCustomerApp.Services;

namespace YogaCustomerApp.Pages
{
    public partial class ClassDetailPage : ContentPage
    {
        private ClassInstance _classInstance;
        private CartService _cartService = CartService.Instance;

        public ClassDetailPage(ClassInstance classInstance)
        {
            InitializeComponent();
            _classInstance = classInstance;
            LoadClassDetails();
        }

        private void LoadClassDetails()
        {
            dateLabel.Text = _classInstance.Date;
            teacherLabel.Text = _classInstance.Teacher;
            
            if (!string.IsNullOrEmpty(_classInstance.Comment))
            {
                commentLabel.Text = _classInstance.Comment;
                commentTitleLabel.IsVisible = true;
                commentLabel.IsVisible = true;
            }
            else
            {
                commentTitleLabel.IsVisible = false;
                commentLabel.IsVisible = false;
            }
        }

        private void OnAddToCartClicked(object sender, EventArgs e)
        {
            _cartService.AddToCart(_classInstance);
        }

        private void OnBackClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }
    }
}
