using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using YogaCustomerApp.Models;

namespace YogaCustomerApp.Services
{
    public class FirestoreService
    {
        private const string BaseUrl = "https://firestore.googleapis.com/v1/projects/yogaappfirebase-90a8c/databases/(default)/documents";
        private const string InstancesCollection = "instances";
        private const string BookingsCollection = "bookings";
        private const string ApiKey = "YOUR_API_KEY";

        public async Task<List<ClassInstance>> GetAllClassInstancesAsync()
        {
            try
            {
                using var client = new HttpClient();
                // Add API key as query parameter for Firestore REST API
                var url = $"{BaseUrl}/{InstancesCollection}?key={ApiKey}";
                System.Diagnostics.Debug.WriteLine($"Fetching from URL: {url}");
                
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Firestore response: {json}");
                    
                    if (string.IsNullOrEmpty(json))
                    {
                        System.Diagnostics.Debug.WriteLine("Empty response from Firestore");
                        return new List<ClassInstance>();
                    }
                    
                    // Check if the response contains an error
                    if (json.Contains("\"error\""))
                    {
                        System.Diagnostics.Debug.WriteLine("Firestore returned an error response");
                        return new List<ClassInstance>();
                    }
                    
                    var result = JsonSerializer.Deserialize<FirestoreResponse>(json);
                    
                    if (result?.Documents != null)
                    {
                        var instances = new List<ClassInstance>();
                        foreach (var doc in result.Documents)
                        {
                            try
                            {
                                var instance = ParseClassInstance(doc);
                                if (instance != null)
                                {
                                    instances.Add(instance);
                                    System.Diagnostics.Debug.WriteLine($"Parsed instance: {instance.Id} - {instance.Date} - {instance.Teacher}");
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error parsing instance: {ex.Message}");
                            }
                        }
                        System.Diagnostics.Debug.WriteLine($"Total instances parsed: {instances.Count}");
                        return instances;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No documents found in response");
                        // Try to parse the response as a different structure
                        if (json.Contains("documents"))
                        {
                            System.Diagnostics.Debug.WriteLine("Response contains 'documents' but parsing failed");
                            // Try alternative parsing
                            return await ParseAlternativeResponse(json);
                        }
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Firestore API error: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in GetAllClassInstancesAsync: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to load classes: {ex.Message}", "OK");
            }

            return new List<ClassInstance>();
        }

        private async Task<List<ClassInstance>> ParseAlternativeResponse(string json)
        {
            try
            {
                // Try to parse as a different structure
                using var document = JsonDocument.Parse(json);
                var instances = new List<ClassInstance>();
                
                if (document.RootElement.TryGetProperty("documents", out var documents))
                {
                    foreach (var doc in documents.EnumerateArray())
                    {
                        try
                        {
                            var instance = ParseClassInstanceFromJson(doc);
                            if (instance != null)
                            {
                                instances.Add(instance);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error parsing alternative instance: {ex.Message}");
                        }
                    }
                }
                
                return instances;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Alternative parsing failed: {ex.Message}");
                return new List<ClassInstance>();
            }
        }

        private ClassInstance ParseClassInstanceFromJson(JsonElement doc)
        {
            try
            {
                if (doc.TryGetProperty("fields", out var fields))
                {
                    var idValue = GetJsonFieldValue(fields, "id");
                    var courseIdValue = GetJsonFieldValue(fields, "courseId");
                    var dateValue = GetJsonFieldValue(fields, "date");
                    var teacherValue = GetJsonFieldValue(fields, "teacher");
                    var commentValue = GetJsonFieldValue(fields, "comment");

                    if (string.IsNullOrEmpty(dateValue) || string.IsNullOrEmpty(teacherValue))
                    {
                        return null;
                    }

                    return new ClassInstance
                    {
                        Id = int.TryParse(idValue, out int id) ? id : 0,
                        CourseId = int.TryParse(courseIdValue, out int courseId) ? courseId : 0,
                        Date = dateValue,
                        Teacher = teacherValue,
                        Comment = commentValue ?? ""
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing JSON instance: {ex.Message}");
            }
            
            return null;
        }

        private string GetJsonFieldValue(JsonElement fields, string fieldName)
        {
            if (fields.TryGetProperty(fieldName, out var field))
            {
                if (field.TryGetProperty("stringValue", out var stringValue))
                    return stringValue.GetString() ?? "";
                if (field.TryGetProperty("integerValue", out var integerValue))
                    return integerValue.GetString() ?? "";
            }
            return "";
        }

        public async Task<List<ClassInstance>> SearchClassInstancesAsync(string dayOfWeek = null, string time = null)
        {
            try
            {
                var allInstances = await GetAllClassInstancesAsync();
                var filtered = allInstances.AsEnumerable();

                if (!string.IsNullOrEmpty(dayOfWeek))
                {
                    filtered = filtered.Where(instance =>
                        instance.Date != null &&
                        GetDayOfWeek(instance.Date).ToLower().Contains(dayOfWeek.ToLower()));
                }

                if (!string.IsNullOrEmpty(time))
                {
                    filtered = filtered.Where(instance =>
                        instance.Date != null &&
                        instance.Date.Contains(time));
                }

                return filtered.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in SearchClassInstancesAsync: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Error", $"Search failed: {ex.Message}", "OK");
                return new List<ClassInstance>();
            }
        }

        public async Task SubmitBooking(string email, List<int> instanceIds)
        {
            try
            {
                var booking = new
                {
                    fields = new
                    {
                        email = new { stringValue = email },
                        instanceIds = new
                        {
                            arrayValue = new
                            {
                                values = instanceIds.Select(id => new { integerValue = id.ToString() }).ToArray()
                            }
                        },
                        timestamp = new { stringValue = DateTime.UtcNow.ToString("o") }
                    }
                };

                var json = JsonSerializer.Serialize(booking);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var client = new HttpClient();
                var url = $"{BaseUrl}/{BookingsCollection}?key={ApiKey}";
                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    await App.Current.MainPage.DisplayAlert("Success", "Booking submitted successfully!", "OK");
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    await App.Current.MainPage.DisplayAlert("Error", $"Failed to submit booking: {error}", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in SubmitBooking: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Error", $"Booking failed: {ex.Message}", "OK");
            }
        }

        public async Task<List<Booking>> GetBookingsByEmailAsync(string email)
        {
            try
            {
                using var client = new HttpClient();
                var url = $"{BaseUrl}/{BookingsCollection}?key={ApiKey}";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<FirestoreResponse>(json);
                    var bookings = result?.Documents?.Select(ParseBooking).ToList() ?? new List<Booking>();
                    return bookings.Where(b => b.Email == email).ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in GetBookingsByEmailAsync: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to load bookings: {ex.Message}", "OK");
            }

            return new List<Booking>();
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var client = new HttpClient();
                var url = $"{BaseUrl}?key={ApiKey}";
                System.Diagnostics.Debug.WriteLine($"Testing connection to: {url}");
                
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                
                System.Diagnostics.Debug.WriteLine($"Test response status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Test response content: {content}");
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Test connection failed: {ex.Message}");
                return false;
            }
        }

        private ClassInstance ParseClassInstance(FirestoreDocument doc)
        {
            try
            {
                if (doc?.Fields == null) return null;

                var fields = doc.Fields;
                
                // Handle both possible field structures
                var idValue = GetFieldValue(fields, "id");
                var courseIdValue = GetFieldValue(fields, "courseId");
                var dateValue = GetFieldValue(fields, "date");
                var teacherValue = GetFieldValue(fields, "teacher");
                var commentValue = GetFieldValue(fields, "comment");

                // Validate required fields
                if (string.IsNullOrEmpty(dateValue) || string.IsNullOrEmpty(teacherValue))
                {
                    System.Diagnostics.Debug.WriteLine($"Skipping instance with missing required fields: date={dateValue}, teacher={teacherValue}");
                    return null;
                }

                var instance = new ClassInstance
                {
                    Id = int.TryParse(idValue, out int id) ? id : 0,
                    CourseId = int.TryParse(courseIdValue, out int courseId) ? courseId : 0,
                    Date = dateValue ?? "",
                    Teacher = teacherValue ?? "",
                    Comment = commentValue ?? ""
                };

                System.Diagnostics.Debug.WriteLine($"Successfully parsed instance: {instance.Id} - {instance.Date} - {instance.Teacher}");
                return instance;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing ClassInstance: {ex.Message}");
                return null;
            }
        }

        private string GetFieldValue(Dictionary<string, FirestoreValue> fields, string fieldName)
        {
            if (fields.TryGetValue(fieldName, out var value))
            {
                if (value == null) return "";
                
                // Check for string value first
                if (!string.IsNullOrEmpty(value.stringValue))
                    return value.stringValue;
                
                // Check for integer value
                if (!string.IsNullOrEmpty(value.integerValue))
                    return value.integerValue;
                
                // Check for double value
                if (!string.IsNullOrEmpty(value.doubleValue))
                    return value.doubleValue;
                
                // Check for boolean value
                if (value.booleanValue.HasValue)
                    return value.booleanValue.Value.ToString();
            }
            return "";
        }

        private Booking ParseBooking(FirestoreDocument doc)
        {
            try
            {
                if (doc?.Fields == null) return null;

                var fields = doc.Fields;
                
                return new Booking
                {
                    Email = GetFieldValue(fields, "email"),
                    InstanceIds = fields.GetValueOrDefault("instanceIds", new FirestoreValue())
                        .arrayValue?.values?.Select(v => int.TryParse(v.integerValue, out int id) ? id : 0).ToList() ?? new List<int>(),
                    Timestamp = GetFieldValue(fields, "timestamp")
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing Booking: {ex.Message}");
                return null;
            }
        }

        private string GetDayOfWeek(string dateString)
        {
            try
            {
                // Try multiple date formats - prioritize dd/MM/yyyy as used by AdminApp
                string[] formats = { "dd/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy", "dd-MM-yyyy" };
                
                foreach (var format in formats)
                {
                    if (DateTime.TryParseExact(dateString, format, null, System.Globalization.DateTimeStyles.None, out DateTime date))
                    {
                        return date.DayOfWeek.ToString();
                    }
                }
                
                // If none of the specific formats work, try general parsing
                if (DateTime.TryParse(dateString, out DateTime generalDate))
                {
                    return generalDate.DayOfWeek.ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing date {dateString}: {ex.Message}");
            }
            
            return "";
        }
    }

    // 🔽 CLASS JSON MAPPING FIRESTORE
    public class FirestoreResponse
    {
        public List<FirestoreDocument> Documents { get; set; } = new List<FirestoreDocument>();
        public string NextPageToken { get; set; } = "";
        public FirestoreError Error { get; set; }
    }

    public class FirestoreError
    {
        public int Code { get; set; }
        public string Message { get; set; } = "";
        public string Status { get; set; } = "";
    }

    public class FirestoreDocument
    {
        public string Name { get; set; } = "";
        public Dictionary<string, FirestoreValue> Fields { get; set; } = new Dictionary<string, FirestoreValue>();
        public string CreateTime { get; set; } = "";
        public string UpdateTime { get; set; } = "";
    }

    public class FirestoreValue
    {
        public string stringValue { get; set; } = "";
        public string integerValue { get; set; } = "";
        public string doubleValue { get; set; } = "";
        public bool? booleanValue { get; set; }
        public FirestoreArray arrayValue { get; set; }
        public Dictionary<string, FirestoreValue> mapValue { get; set; }
    }

    public class FirestoreArray
    {
        public List<FirestoreValue> values { get; set; } = new List<FirestoreValue>();
    }
}
