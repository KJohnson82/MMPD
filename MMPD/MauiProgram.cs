using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using MMPD.Data.Context;
using MMPD.Services;
using MMPD.Shared.Services;

#if WINDOWS
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT.Interop;
using Microsoft.UI;
#endif

namespace MMPD
{
    /// <summary>
    /// The main entry point for the .NET MAUI application.
    /// This class is responsible for creating and configuring the application,
    /// including setting up services, fonts, and platform-specific lifecycle events.
    /// </summary>
    public static class MauiProgram
    {
        /// <summary>
        /// Creates and configures the MauiApp instance.
        /// This method sets up dependency injection, registers services, configures the database,
        /// and initializes the application's main components.
        /// </summary>
        /// <returns>A configured MauiApp instance ready to be run.</returns>
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // Configure platform-specific lifecycle events.
            builder.ConfigureLifecycleEvents(events =>
            {
#if WINDOWS
                // Windows-specific configurations for the application window.
                events.AddWindows(windowsLifecycleBuilder =>
                {
                    windowsLifecycleBuilder.OnWindowCreated(window =>
                    {
                        // Ensure the default title bar is used instead of a custom one.
                        window.ExtendsContentIntoTitleBar = false;

                        // Use the dispatcher queue to modify the AppWindow after it has been fully initialized.
                        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
                        {
                            var handle = WindowNative.GetWindowHandle(window);
                            var id = Win32Interop.GetWindowIdFromWindow(handle);
                            var appWindow = AppWindow.GetFromWindowId(id);

                            // Access the window's presenter to control properties like maximizability.
                            if (appWindow?.Presenter is OverlappedPresenter overlappedPresenter)
                            {
                                // Disable the maximize button on the window.
                                overlappedPresenter.IsMaximizable = false;
                            }
                        });
                    });
                });
#endif
            });


            // ===== SERVICE REGISTRATION (DEPENDENCY INJECTION) =====

            // Register device-specific services used by shared projects.
            builder.Services.AddSingleton<IFormFactor, FormFactor>();

            // Register Telerik UI for Blazor components.
            builder.Services.AddTelerikBlazor();

            // Add the BlazorWebView to host Blazor components in the MAUI app.
            builder.Services.AddMauiBlazorWebView();

            // Register custom application services.
            builder.Services.AddScoped<MMPD.Shared.Services.SearchService>();
            builder.Services.AddScoped<IDirectoryService, DirectoryService>();
            builder.Services.AddSingleton<LayoutState>();

            // ===== DATABASE SETUP =====

            // Determine the writable path for the application's database file.
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "mcelroy_directory.db");

            // Ensure the database is seeded (copied from assets) before the DbContext is used.
            SeedDatabase(dbPath);

            // Register the AppDbContext with the dependency injection container.
            // Configures it to use SQLite with the specified database file path.
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite($"Data Source={dbPath}");
            });

#if DEBUG
            // Enable Blazor WebView developer tools in debug builds.
            builder.Services.AddBlazorWebViewDeveloperTools();
            // Add debug logging provider.
            builder.Logging.AddDebug();
#endif

            // Build and return the configured application.
            return builder.Build();
        }

        /// <summary>
        /// Handles the initial setup of the application's SQLite database.
        /// It copies a pre-populated database from the project's assets to a writable
        /// application data directory if it doesn't already exist.
        /// In DEBUG mode, it will overwrite the existing database on each launch.
        /// </summary>
        /// <param name="dbPath">The full file path for the target database.</param>
        static void SeedDatabase(string dbPath)
        {
#if DEBUG
            // In debug mode, delete the old database to ensure the latest version from assets is used.
            // This is useful for development when the schema or seed data changes.
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
#endif

            // This check runs on every launch in Debug (since the file is deleted)
            // or only on the first launch in Release mode.
            if (!File.Exists(dbPath))
            {
                // Open the embedded database file as a read-only stream.
                using var dbStream = File.OpenRead("Data/mcelroy_directory.db");
                // Create a new file stream in the app's data directory.
                using var fileStream = new FileStream(dbPath, FileMode.Create, FileAccess.Write);
                // Copy the contents from the embedded database to the new file.
                dbStream.CopyTo(fileStream);
            }
        }
    }
}
