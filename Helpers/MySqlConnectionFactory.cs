using MySqlConnector;

namespace AgriConnect.Helpers
{
    /// <summary>
    /// Factory for creating MySQL connections to the XAMPP-hosted agriconnect_db database.
    /// Update Uid/Pwd if your XAMPP MySQL uses a password.
    /// </summary>
    public static class MySqlConnectionFactory
    {
        private const string ConnectionString =
            "Server=localhost;Port=3306;Database=agriconnect_db;Uid=root;Pwd=;";

        /// <summary>Returns a new, closed MySqlConnection. Caller must open and dispose it.</summary>
        public static MySqlConnection CreateConnection()
            => new MySqlConnection(ConnectionString);
    }
}
