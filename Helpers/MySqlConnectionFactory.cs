using MySqlConnector;

namespace AgriConnect.Helpers
{
    /// <summary>
    /// Factory for creating MySQL connections to the XAMPP-hosted agriconnect_db database.
    /// Default XAMPP settings: localhost, port 3306, user 'root', empty password.
    /// </summary>
    public static class MySqlConnectionFactory
    {
        public const string ConnectionString =
            "Server=localhost;Port=3306;Database=agriconnect_db;Uid=root;Pwd=;AllowUserVariables=True;";

        /// <summary>Returns a new, closed MySqlConnection. Caller must open and dispose it.</summary>
        public static MySqlConnection CreateConnection()
            => new MySqlConnection(ConnectionString);

        /// <summary>Tests if MySQL server is reachable.</summary>
        public static bool TestConnection(out string? error)
        {
            try
            {
                using var conn = CreateConnection();
                conn.Open();
                error = null;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
