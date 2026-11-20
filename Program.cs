using AgriConnect.Repositories;
using AgriConnect.Views;

namespace AgriConnect
{
    internal static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            if (args.Length > 0 && args[0].Equals("--test", StringComparison.OrdinalIgnoreCase))
            {
                return AgriConnectTests.RunAllTests();
            }

            if (args.Length > 0 && args[0].Equals("--init-mysql", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var repo = new StudentRepository();
                    repo.EnsureTableExists();
                    Console.WriteLine("SUCCESS: students table created in agriconnect_db");
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                    return 1;
                }
            }

            // Ensure MySQL table is created on app startup
            try
            {
                var repo = new StudentRepository();
                repo.EnsureTableExists();
            }
            catch
            {
                // Ignore if MySQL service isn't started yet
            }

            ApplicationConfiguration.Initialize();
            // Start directly with Form1 so the user sees the MySQL students form immediately!
            Application.Run(new Form1());
            return 0;
        }
    }
}
