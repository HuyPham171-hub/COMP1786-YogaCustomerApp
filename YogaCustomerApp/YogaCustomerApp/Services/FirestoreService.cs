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

        public async Task<List<ClassInstance>> GetAllClassInstancesAsync()
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync($"{BaseUrl}/{InstancesCollection}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Firestore Response: {json}");
                    
                    var result = JsonSerializer.Deserialize<FirestoreResponse>(json);
                    var instances = new List<ClassInstance>();
                    
                    if (result?.documents != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found {result.documents.Count} documents");
                        
                        foreach (var doc in result.documents)
                        {
                            try
                            {
                                System.Diagnostics.Debug.WriteLine($"Parsing document: {JsonSerializer.Serialize(doc)}");
                                var instance = ParseClassInstance(doc);
                                if (instance != null)
                                {
                                    instances.Add(instance);
                                    System.Diagnostics.Debug.WriteLine($"Successfully parsed instance: ID={instance.Id}, Date={instance.Date}, Teacher={instance.Teacher}");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine("ParseClassInstance returned null");
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error parsing document: {ex.Message}");
                            }
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No documents found in response");
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Total instances parsed: {instances.Count}");
                    return instances;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Firestore Error: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FirestoreService Error: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to load classes: {ex.Message}", "OK");
            }

            return new List<ClassInstance>();
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
                        GetDayOfWeek(instance.Date).Equals(dayOfWeek, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(time))
                {
                    filtered = filtered.Where(instance =>
                        instance.Date != null &&
                        GetTimeFromDate(instance.Date).Contains(time));
                }

                return filtered.ToList();
            }
            catch (Exception ex)
            {
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
                var response = await client.PostAsync($"{BaseUrl}/{BookingsCollection}", content);

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
                await App.Current.MainPage.DisplayAlert("Error", $"Booking failed: {ex.Message}", "OK");
            }
        }

        public async Task<List<Booking>> GetBookingsByEmailAsync(string email)
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync($"{BaseUrl}/{BookingsCollection}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<FirestoreResponse>(json);
                    var bookings = result?.documents?.Select(ParseBooking).ToList() ?? new List<Booking>();
                    return bookings.Where(b => b.Email == email).ToList();
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to load bookings: {ex.Message}", "OK");
            }

            return new List<Booking>();
        }

        private ClassInstance ParseClassInstance(FirestoreDocument doc)
        {
            try
            {
                var instance = new ClassInstance();
                
                if (doc.fields != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Parsing fields: {JsonSerializer.Serialize(doc.fields)}");
                    
                    // Parse Id
                    if (doc.fields.ContainsKey("id") && doc.fields["id"]?.integerValue != null)
                    {
                        if (int.TryParse(doc.fields["id"].integerValue, out int id))
                        {
                            instance.Id = id;
                            System.Diagnostics.Debug.WriteLine($"Parsed ID: {id}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to parse ID: {doc.fields["id"].integerValue}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("ID field not found or null");
                    }
                    
                    // Parse CourseId
                    if (doc.fields.ContainsKey("courseId") && doc.fields["courseId"]?.integerValue != null)
                    {
                        if (int.TryParse(doc.fields["courseId"].integerValue, out int courseId))
                        {
                            instance.CourseId = courseId;
                            System.Diagnostics.Debug.WriteLine($"Parsed CourseId: {courseId}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to parse CourseId: {doc.fields["courseId"].integerValue}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("CourseId field not found or null");
                    }
                    
                    // Parse Date
                    if (doc.fields.ContainsKey("date") && doc.fields["date"]?.stringValue != null)
                    {
                        instance.Date = doc.fields["date"].stringValue;
                        System.Diagnostics.Debug.WriteLine($"Parsed Date: {instance.Date}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Date field not found or null");
                        instance.Date = ""; // Set empty string if no date
                    }
                    
                    // Parse Teacher
                    if (doc.fields.ContainsKey("teacher") && doc.fields["teacher"]?.stringValue != null)
                    {
                        instance.Teacher = doc.fields["teacher"].stringValue;
                        System.Diagnostics.Debug.WriteLine($"Parsed Teacher: {instance.Teacher}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Teacher field not found or null");
                        instance.Teacher = ""; // Set empty string if no teacher
                    }
                    
                    // Parse Comment
                    if (doc.fields.ContainsKey("comment") && doc.fields["comment"]?.stringValue != null)
                    {
                        instance.Comment = doc.fields["comment"].stringValue;
                        System.Diagnostics.Debug.WriteLine($"Parsed Comment: {instance.Comment}");
                    }
                    else
                    {
                        instance.Comment = ""; // Set empty string if no comment
                        System.Diagnostics.Debug.WriteLine("Comment field not found or null, setting empty string");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Document fields is null");
                    return null;
                }
                
                // Validate that we have at least the required fields
                if (instance.Id == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Instance ID is 0, skipping this instance");
                    return null;
                }
                
                System.Diagnostics.Debug.WriteLine($"Successfully created instance: ID={instance.Id}, Date={instance.Date}, Teacher={instance.Teacher}");
                return instance;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing ClassInstance: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        private Booking ParseBooking(FirestoreDocument doc)
        {
            try
            {
                var booking = new Booking();
                
                if (doc.fields != null)
                {
                    // Parse Email
                    if (doc.fields.ContainsKey("email") && doc.fields["email"]?.stringValue != null)
                    {
                        booking.Email = doc.fields["email"].stringValue;
                    }
                    
                    // Parse InstanceIds
                    if (doc.fields.ContainsKey("instanceIds") && doc.fields["instanceIds"]?.arrayValue?.values != null)
                    {
                        booking.InstanceIds = new List<int>();
                        foreach (var value in doc.fields["instanceIds"].arrayValue.values)
                        {
                            if (value?.integerValue != null && int.TryParse(value.integerValue, out int id))
                            {
                                booking.InstanceIds.Add(id);
                            }
                        }
                    }
                    else
                    {
                        booking.InstanceIds = new List<int>();
                    }
                    
                    // Parse Timestamp
                    if (doc.fields.ContainsKey("timestamp") && doc.fields["timestamp"]?.stringValue != null)
                    {
                        booking.Timestamp = doc.fields["timestamp"].stringValue;
                    }
                }
                
                return booking;
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
                // Try to parse the date in dd/MM/yyyy format
                if (DateTime.TryParseExact(dateString, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime date))
                {
                    return date.DayOfWeek.ToString();
                }
                
                // Fallback to default parsing
                if (DateTime.TryParse(dateString, out DateTime fallbackDate))
                {
                    return fallbackDate.DayOfWeek.ToString();
                }
                
                System.Diagnostics.Debug.WriteLine($"Failed to parse date: {dateString}");
                return "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing date {dateString}: {ex.Message}");
                return "";
            }
        }

        private string GetTimeFromDate(string dateString)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Extracting time from date: {dateString}");
                
                // Try to parse with time format dd/MM/yyyy HH:mm
                if (DateTime.TryParseExact(dateString, "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime dateTime))
                {
                    var time = dateTime.ToString("HH:mm");
                    System.Diagnostics.Debug.WriteLine($"Extracted time: {time}");
                    return time;
                }
                
                // Try to parse with time format dd/MM/yyyy H:mm
                if (DateTime.TryParseExact(dateString, "dd/MM/yyyy H:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime dateTime2))
                {
                    var time = dateTime2.ToString("HH:mm");
                    System.Diagnostics.Debug.WriteLine($"Extracted time: {time}");
                    return time;
                }
                
                // Try to parse the date in dd/MM/yyyy format (no time)
                if (DateTime.TryParseExact(dateString, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime date))
                {
                    System.Diagnostics.Debug.WriteLine($"Date has no time component: {dateString}");
                    return ""; // No time component
                }
                
                // Fallback to default parsing
                if (DateTime.TryParse(dateString, out DateTime fallbackDate))
                {
                    var time = fallbackDate.ToString("HH:mm");
                    System.Diagnostics.Debug.WriteLine($"Extracted time from fallback: {time}");
                    return time;
                }
                
                System.Diagnostics.Debug.WriteLine($"Failed to parse date for time extraction: {dateString}");
                return "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting time from date {dateString}: {ex.Message}");
                return "";
            }
        }
    }

    // Helper classes for JSON deserialization
    public class FirestoreResponse
    {
        public List<FirestoreDocument> documents { get; set; }
    }

    public class FirestoreDocument
    {
        public Dictionary<string, FirestoreValue> fields { get; set; }
    }

    public class FirestoreValue
    {
        public string stringValue { get; set; }
        public string integerValue { get; set; }
        public FirestoreArray arrayValue { get; set; }
    }

    public class FirestoreArray
    {
        public List<FirestoreValue> values { get; set; }
    }
}
