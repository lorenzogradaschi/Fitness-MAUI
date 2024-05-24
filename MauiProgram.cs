using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using DSR; // Assuming DatabaseHelper is in this namespace

namespace DSR
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif

            // Initialize Database and roles at app startup asynchronously in a non-blocking way
            Task.Run(() =>
            {
                DatabaseHelper.InitializeDatabase();
                DatabaseHelper.InitializeRolesAndAdminUser();
            });

            // Register services and view models
            // builder.Services.AddSingleton<YourDataService>();
            // builder.Services.AddTransient<YourViewModel>();

            return builder.Build(); // Builds the MauiApp with the configured settings
        }
    }
}
