using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YogaCustomerApp.Models
{
    public class Booking
    {
        public string Email { get; set; }
        public List<int> InstanceIds { get; set; }
        public string Timestamp { get; set; }
    }
}
