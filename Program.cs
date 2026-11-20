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
                    AppDatabase.Instance.InitializeMySql();
                    Console.WriteLine("SUCCESS: All AgriConnect tables initialized in agriconnect_db");
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                    return 1;
                }
            }

            // Initialize database & sync with MySQL in XAMPP
            try
            {
                AppDatabase.Instance.InitializeMySql();
            }
            catch
            {
                // Graceful fallback if MySQL service isn't started yet
            }

            ApplicationConfiguration.Initialize();
            // Start with Form1 (Produce & Inventory Manager linked to agriconnect_db)
            Application.Run(new Form1());
            return 0;
        }
    }
}
