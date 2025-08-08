namespace YogaCustomerApp
{
	public static class Constants
	{
		// Firebase Firestore Collections
		public const string CoursesCollection = "courses";
		public const string InstancesCollection = "instances";
		public const string BookingsCollection = "bookings";

		// Validation
		public const int MaxCartItems = 10;

		// Firestore field names
		public const string FieldId = "id";
		public const string FieldCourseId = "courseId";
		public const string FieldDate = "date";
		public const string FieldTeacher = "teacher";
		public const string FieldComment = "comment";
		public const string FieldEmail = "email";
		public const string FieldBookedItems = "bookedItems";
	}
}
