namespace AgriConnect.Models
{
    /// <summary>Represents a row in the MySQL `students` table.</summary>
    public class Student
    {
        public int Id            { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Course     { get; set; } = string.Empty;
        public string Email      { get; set; } = string.Empty;
        public DateTime EnrolledDate { get; set; }
    }
}
