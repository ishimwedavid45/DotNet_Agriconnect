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

            ApplicationConfiguration.Initialize();
            Application.Run(new LoginForm());
            return 0;
        }
    }
}
