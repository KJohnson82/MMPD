using Microsoft.Extensions.Logging;
using MMPD.Services;
using MMPD.Shared.Services;
using MMPD.Data.Context;
using Microsoft.EntityFrameworkCore;
using MMPD.Data.Data;
using Microsoft.Maui.LifecycleEvents;

namespace MMPD
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
                });

            builder.ConfigureLifecycleEvents(events =>
            {

            });

            // Add device-specific services used by the MMPD.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();

            builder.Services.AddTelerikBlazor();

            builder.Services.AddMauiBlazorWebView();

            // 🔸 Get writable path
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "mcelroy_directory.db");

            // ==> CALL THE SEED METHOD HERE <==
            SeedDatabase(dbPath);

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite($"Data Source={dbPath}");
            });

            


            builder.Services.AddScoped<IDirectoryService, DirectoryService>();
            builder.Services.AddSingleton<LayoutState>();


#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
        // Add this helper method to the MauiProgram class
        static void SeedDatabase(string dbPath)
        {
#if DEBUG
            // In debug mode, we delete the old database to ensure
            // the new, updated version from our project is copied.
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
#endif

            // This now runs every time in Debug, or only once if in Release mode.
            if (!File.Exists(dbPath))
            {
                using var dbStream = File.OpenRead("Data/mcelroy_directory.db");
                using var fileStream = new FileStream(dbPath, FileMode.Create, FileAccess.Write);
                dbStream.CopyTo(fileStream);
            }
        }
    }
}
