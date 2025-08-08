namespace YogaCustomerApp.Models
{
    public class CartItem
    {
        public int InstanceId { get; set; }
        public int CourseId { get; set; }
        public string Date { get; set; }
        public string Teacher { get; set; }

        public string DisplayText => $"{Date} - {Teacher}";
        public string ShortDisplayText => $"{Date?.Split(' ')[0]} - {Teacher}";

        public override string ToString()
        {
            return DisplayText;
        }
    }
}
