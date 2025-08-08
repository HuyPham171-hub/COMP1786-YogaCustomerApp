using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YogaCustomerApp.Models
{
    public class Course
    {
        public int Id { get; set; }               // Unique ID of the course
        public string Type { get; set; }          // e.g., Hatha, Vinyasa, etc.
        public string DayOfWeek { get; set; }     // e.g., Monday, Tuesday
        public string Time { get; set; }          // e.g., 09:00 AM
        public int Capacity { get; set; }         // Max number of students
        public int Duration { get; set; }         // Duration in minutes
        public string SkillLevel { get; set; }    // e.g., Beginner, Advanced
        public double Price { get; set; }         // Price of the course
        public string Description { get; set; }   // Optional description
    }
}

