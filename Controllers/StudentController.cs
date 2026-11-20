using AgriConnect.Models;
using AgriConnect.Repositories;

namespace AgriConnect.Controllers
{
    /// <summary>
    /// Controller that mediates between Form1 (View) and StudentRepository (Model).
    /// Handles validation and delegates persistence to the repository.
    /// </summary>
    public class StudentController
    {
        private readonly StudentRepository _repo;

        public StudentController()
        {
            _repo = new StudentRepository();
            // Ensure the students table exists on first use
            _repo.EnsureTableExists();
        }

        // ------------------------------------------------------------------ //
        //  Save a new student (validate → insert → return refreshed list)
        // ------------------------------------------------------------------ //
        /// <summary>
        /// Validates inputs, inserts the student row, then returns the full
        /// updated student list so the View can refresh its DataGridView.
        /// Throws ArgumentException with a user-friendly message on bad input.
        /// </summary>
        public List<Student> SaveStudent(string name, string course, string email)
        {
            // --- validation ---
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Student Name is required.");

            if (string.IsNullOrWhiteSpace(course))
                throw new ArgumentException("Course is required.");

            if (!string.IsNullOrEmpty(email) && !email.Contains('@'))
                throw new ArgumentException("Email address is not valid.");

            // --- insert ---
            var student = new Student
            {
                StudentName = name.Trim(),
                Course      = course.Trim(),
                Email       = email.Trim(),
            };
            _repo.Add(student);

            // --- return refreshed list ---
            return _repo.GetAll();
        }

        // ------------------------------------------------------------------ //
        //  Load all students
        // ------------------------------------------------------------------ //
        /// <summary>Returns all students for the initial DataGridView load.</summary>
        public List<Student> GetAllStudents() => _repo.GetAll();
    }
}
