using System.Collections.ObjectModel;
using YogaCustomerApp.Models;
using YogaCustomerApp.Services;

namespace YogaCustomerApp.Pages
{
    public partial class ClassListPage : ContentPage
    {
        public ObservableCollection<ClassInstance> ClassInstances { get; set; } = new();
        private FirestoreService _firestoreService = new();
        private CartService _cartService = CartService.Instance;

        public ClassListPage()
        {
            InitializeComponent();
            BindingContext = this;
            // Set the ItemsSource directly to the CollectionView
            classListView.ItemsSource = ClassInstances;
            
            // Add some debugging
            System.Diagnostics.Debug.WriteLine("ClassListPage initialized");
            System.Diagnostics.Debug.WriteLine($"Initial ClassInstances count: {ClassInstances.Count}");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadClasses();
        }

        private async Task LoadClasses()
        {
            try
            {
                statusLabel.Text = "Loading classes...";
                System.Diagnostics.Debug.WriteLine("Starting LoadClasses...");
                
                // Check network connectivity
                if (!NetworkUtil.IsConnected())
                {
                    await DisplayAlert("No Internet", "Please check your connection and try again.", "OK");
                    statusLabel.Text = "No internet connection";
                    System.Diagnostics.Debug.WriteLine("Network not connected");
                    return;
                }

                // Additional internet check
                if (!await NetworkUtil.CheckInternetConnectionAsync())
                {
                    await DisplayAlert("No Internet", "Cannot reach the internet. Please check your connection.", "OK");
                    statusLabel.Text = "No internet connection";
                    System.Diagnostics.Debug.WriteLine("Internet connection check failed");
                    return;
                }

                // Test Firestore connection
                System.Diagnostics.Debug.WriteLine("Testing Firestore connection...");
                var connectionTest = await _firestoreService.TestConnectionAsync();
                if (!connectionTest)
                {
                    await DisplayAlert("Connection Error", "Cannot connect to the cloud service. Please try again later.", "OK");
                    statusLabel.Text = "Connection failed";
                    System.Diagnostics.Debug.WriteLine("Firestore connection test failed");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("Network checks passed, fetching classes...");
                var classes = await _firestoreService.GetAllClassInstancesAsync();
                System.Diagnostics.Debug.WriteLine($"Retrieved {classes.Count} classes from Firestore");
                
                // Clear and reload on UI thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ClassInstances.Clear();
                    foreach (var item in classes)
                    {
                        ClassInstances.Add(item);
                        System.Diagnostics.Debug.WriteLine($"Added to UI: {item.Id} - {item.Date} - {item.Teacher}");
                    }
                    statusLabel.Text = $"Found {ClassInstances.Count} classes";
                    System.Diagnostics.Debug.WriteLine($"UI updated with {ClassInstances.Count} classes");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadClasses error: {ex}");
                await DisplayAlert("Error", $"Failed to load classes: {ex.Message}", "OK");
                statusLabel.Text = "Error loading classes";
            }
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            try
            {
                string day = entrySearchDay.Text?.Trim();
                string time = entrySearchTime.Text?.Trim();

                if (string.IsNullOrEmpty(day) && string.IsNullOrEmpty(time))
                {
                    await LoadClasses();
                    return;
                }

                statusLabel.Text = "Searching...";
                var filtered = await _firestoreService.SearchClassInstancesAsync(day, time);
                
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ClassInstances.Clear();
                    foreach (var item in filtered)
                    {
                        ClassInstances.Add(item);
                    }
                    statusLabel.Text = $"Found {ClassInstances.Count} matching classes";
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Search failed: {ex.Message}", "OK");
                statusLabel.Text = "Search failed";
                System.Diagnostics.Debug.WriteLine($"Search error: {ex}");
            }
        }

        private async void OnClearSearchClicked(object sender, EventArgs e)
        {
            entrySearchDay.Text = "";
            entrySearchTime.Text = "";
            await LoadClasses();
        }

        private void OnClassSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ClassInstance selected)
            {
                Navigation.PushAsync(new ClassDetailPage(selected));
            }
        }

        private void OnAddToCartClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is ClassInstance instance)
            {
                _cartService.AddToCart(instance);
            }
        }
    }

    // Helper converter for visibility
    public class StringToBoolConverter : IValueConverter
    {
        public static StringToBoolConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !string.IsNullOrEmpty(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
