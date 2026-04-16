using ECommerce.DL;
using ECommerce.Utilities;

namespace ECommerce.UI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Initialize database connection
            DbHelper.Initialize(AppConfig.ConnectionString);
            Logger.LogInfo("Application started.");

            Application.Run(new Forms.LoginForm());
        }
    }
}
