using System.Net.NetworkInformation;

namespace YogaCustomerApp.Services
{
    public static class NetworkUtil
    {
        public static bool IsConnected()
        {
            try
            {
                // Check if network interface is available
                if (!NetworkInterface.GetIsNetworkAvailable())
                    return false;

                // Check if any network interface is up
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                return interfaces.Any(ni => ni.OperationalStatus == OperationalStatus.Up && 
                                          (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || 
                                           ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NetworkUtil.IsConnected error: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> CheckInternetConnectionAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var response = await client.GetAsync("https://www.google.com");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NetworkUtil.CheckInternetConnectionAsync error: {ex.Message}");
                return false;
            }
        }
    }
}
