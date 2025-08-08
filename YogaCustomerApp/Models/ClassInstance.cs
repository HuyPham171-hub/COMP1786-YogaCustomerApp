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
    }
}

