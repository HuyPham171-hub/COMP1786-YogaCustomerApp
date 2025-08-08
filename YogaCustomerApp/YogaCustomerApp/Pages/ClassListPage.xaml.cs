using System.Collections.ObjectModel;
using YogaCustomerApp.Models;
using YogaCustomerApp.Services;

namespace YogaCustomerApp.Pages
{
    public partial class ClassListPage : ContentPage
    {
        private List<ClassInstance> _allClasses;
        private string _selectedDay = "";
        private string _selectedTime = "";

        public ClassListPage()
        {
            InitializeComponent();
            LoadClasses();
        }

        private async void LoadClasses()
        {
            statusLabel.Text = "Loading classes...";
            var firestoreService = new FirestoreService();
            _allClasses = await firestoreService.GetAllClassInstancesAsync();
            classListView.ItemsSource = _allClasses;
            statusLabel.Text = $"Found {_allClasses.Count} classes";
        }

        private void OnDaySelected(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            if (picker.SelectedIndex >= 0)
            {
                _selectedDay = picker.Items[picker.SelectedIndex];
                if (_selectedDay == "All Days") _selectedDay = "";
            }
        }

        private void OnTimeSelected(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            if (picker.SelectedIndex >= 0)
            {
                _selectedTime = picker.Items[picker.SelectedIndex];
            }
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            statusLabel.Text = "Searching...";
            
            // Debug information
            System.Diagnostics.Debug.WriteLine($"Searching with Day: '{_selectedDay}', Time: '{_selectedTime}'");
            
            var firestoreService = new FirestoreService();
            var results = await firestoreService.SearchClassInstancesAsync(_selectedDay, _selectedTime);
            
            classListView.ItemsSource = results;
            
            // Show search criteria in status
            var searchCriteria = new List<string>();
            if (!string.IsNullOrEmpty(_selectedDay)) searchCriteria.Add($"Day: {_selectedDay}");
            if (!string.IsNullOrEmpty(_selectedTime)) searchCriteria.Add($"Time: {_selectedTime}");
            
            if (searchCriteria.Count > 0)
            {
                statusLabel.Text = $"Found {results.Count} classes ({string.Join(", ", searchCriteria)})";
            }
            else
            {
                statusLabel.Text = $"Found {results.Count} classes (all)";
            }
            
            System.Diagnostics.Debug.WriteLine($"Search completed. Found {results.Count} results.");
        }

        private void OnClearSearchClicked(object sender, EventArgs e)
        {
            pickerDay.SelectedIndex = -1;
            pickerTime.SelectedIndex = -1;
            _selectedDay = "";
            _selectedTime = "";
            classListView.ItemsSource = _allClasses;
            statusLabel.Text = $"Found {_allClasses.Count} classes";
        }

        private async void OnClassSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ClassInstance selectedClass)
            {
                await Navigation.PushAsync(new ClassDetailPage(selectedClass));
            }
        }

        private void OnAddToCartClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var classInstance = (ClassInstance)button.BindingContext;

            CartService.Instance.AddToCart(classInstance);
            DisplayAlert("Success", "Added to cart!", "OK");
        }
    }
}
