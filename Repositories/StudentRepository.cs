using AgriConnect.Helpers;
using AgriConnect.Models;
using MySqlConnector;

namespace AgriConnect.Repositories
{
    /// <summary>
    /// Data-access layer for the MySQL `students` table.
    /// All queries use parameterized commands — no string concatenation.
    /// </summary>
    public class StudentRepository
    {
        // ------------------------------------------------------------------ //
        //  DDL – create table if it doesn't exist yet
        // ------------------------------------------------------------------ //
        public void EnsureTableExists()
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS students (
                    Id           INT AUTO_INCREMENT PRIMARY KEY,
                    StudentName  VARCHAR(100)  NOT NULL,
                    Course       VARCHAR(100)  NOT NULL,
                    Email        VARCHAR(150),
                    EnrolledDate DATETIME DEFAULT CURRENT_TIMESTAMP
                );";

            using var conn = MySqlConnectionFactory.CreateConnection();
            conn.Open();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        // ------------------------------------------------------------------ //
        //  INSERT  — parameterized to prevent SQL injection
        // ------------------------------------------------------------------ //
        /// <summary>Inserts a new student row. Returns the new auto-increment Id.</summary>
        public int Add(Student student)
        {
            const string sql = @"
                INSERT INTO students (StudentName, Course, Email)
                VALUES (@name, @course, @email);
                SELECT LAST_INSERT_ID();";

            using var conn = MySqlConnectionFactory.CreateConnection();
            conn.Open();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name",   student.StudentName);
            cmd.Parameters.AddWithValue("@course", student.Course);
            cmd.Parameters.AddWithValue("@email",  student.Email);

            var result = cmd.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        // ------------------------------------------------------------------ //
        //  SELECT ALL
        // ------------------------------------------------------------------ //
        /// <summary>Returns every row from the students table, ordered by Id.</summary>
        public List<Student> GetAll()
        {
            const string sql = "SELECT Id, StudentName, Course, Email, EnrolledDate FROM students ORDER BY Id;";

            var list = new List<Student>();

            using var conn = MySqlConnectionFactory.CreateConnection();
            conn.Open();
            using var cmd    = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Student
                {
                    Id           = reader.GetInt32("Id"),
                    StudentName  = reader.GetString("StudentName"),
                    Course       = reader.GetString("Course"),
                    Email        = reader.IsDBNull(reader.GetOrdinal("Email")) ? "" : reader.GetString("Email"),
                    EnrolledDate = reader.GetDateTime("EnrolledDate"),
                });
            }

            return list;
        }
    }
}
