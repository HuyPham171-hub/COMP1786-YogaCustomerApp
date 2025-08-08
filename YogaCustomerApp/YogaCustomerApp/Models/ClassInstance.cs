using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YogaCustomerApp.Models
{
    public class ClassInstance
    {
        public int Id { get; set; }               // Unique ID of the instance
        public int CourseId { get; set; }         // Reference to the course
        public string Date { get; set; }          // Date in format "YYYY-MM-DD"
        public string Teacher { get; set; }       // Name of the instructor
        public string Comment { get; set; }       // Optional comment or notes
        
        // Computed property to extract time from date
        public string TimeDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(Date)) return "";
                
                try
                {
                    // Try to parse with time format dd/MM/yyyy HH:mm
                    if (DateTime.TryParseExact(Date, "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime dateTime))
                    {
                        return dateTime.ToString("HH:mm");
                    }
                    
                    // Try to parse with time format dd/MM/yyyy H:mm
                    if (DateTime.TryParseExact(Date, "dd/MM/yyyy H:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime dateTime2))
                    {
                        return dateTime2.ToString("HH:mm");
                    }
                    
                    return ""; // No time component
                }
                catch
                {
                    return "";
                }
            }
        }
        
        // Computed property to extract date only (without time)
        public string DateDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(Date)) return "";
                
                try
                {
                    // Try to parse with time format dd/MM/yyyy HH:mm
                    if (DateTime.TryParseExact(Date, "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime dateTime))
                    {
                        return dateTime.ToString("dd/MM/yyyy");
                    }
                    
                    // Try to parse with time format dd/MM/yyyy H:mm
                    if (DateTime.TryParseExact(Date, "dd/MM/yyyy H:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime dateTime2))
                    {
                        return dateTime2.ToString("dd/MM/yyyy");
                    }
                    
                    // Try to parse the date in dd/MM/yyyy format (no time)
                    if (DateTime.TryParseExact(Date, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime date))
                    {
                        return date.ToString("dd/MM/yyyy");
                    }
                    
                    return Date; // Return original if parsing fails
                }
                catch
                {
                    return Date; // Return original if parsing fails
                }
            }
        }
    }
}

