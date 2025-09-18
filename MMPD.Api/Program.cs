using Microsoft.EntityFrameworkCore;
using MMPD.Data.Context;
using MMPD.Data.Data;

var builder = WebApplication.CreateBuilder(args);

// ===== SERVICE REGISTRATION (DEPENDENCY INJECTION) =====

// --- CORS Configuration ---
// Configures Cross-Origin Resource Sharing (CORS) to allow requests from specific origins.
// This is essential for web applications hosted on different domains to interact with the API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("McElroyPolicy", policy =>
    {
        policy
            // Define the list of allowed client application origins.
            .WithOrigins(
                "https://localhost:7141", // Example: Blazor Web App
                "https://localhost:5067", // Example: Another client
                "http://localhost:5067"
            )
            .AllowAnyHeader()   // Allows any HTTP header in the request.
            .AllowAnyMethod()   // Allows any HTTP method (GET, POST, PUT, etc.).
            .AllowCredentials(); // Allows credentials (e.g., cookies, authorization headers).
    });
});


// --- Database Configuration ---
// Registers the AppDbContext with the dependency injection container.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Retrieve the database connection string from appsettings.json.
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlite(connectionString);

    // Enable detailed error messages and sensitive data logging in development for easier debugging.
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// --- Register Application Services ---
// Makes custom services available for injection into controllers and other services.
builder.Services.AddScoped<IDirectoryService, DirectoryService>();
builder.Services.AddScoped<ExportData>();

// --- API & OpenAPI Configuration ---
// Sets up services required for controllers and API endpoint discovery.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Needed for OpenAPI/Swagger.
builder.Services.AddOpenApi(); // Uses the new .NET 9 built-in OpenAPI generator.

// Build the application host.
var app = builder.Build();

// ===== HTTP REQUEST PIPELINE CONFIGURATION =====

// --- Development-Specific Configuration ---
// Configures middleware that should only run in the development environment.
if (app.Environment.IsDevelopment())
{
    // Exposes the generated OpenAPI specification as a JSON endpoint.
    app.MapOpenApi();

    // Adds a simple root endpoint that provides a basic HTML page with API documentation and links.
    // This serves as a quick reference for developers.
    app.MapGet("/", () => Results.Content(
        """
        <html>
        <head><title>McElroy Directory API</title></head>
        <body style="font-family: Arial, sans-serif; margin: 40px;">
            <h1>?? McElroy Directory API</h1>
            <p>Your API is running successfully!</p>
            
            <h2>?? Available Endpoints:</h2>
            <ul>
                <li><strong>GET /health</strong> - Health check (no API key required)</li>
                <li><strong>GET /api/Directory/sync?apiKey=maui-app-key-2024</strong> - Full directory sync</li>
                <li><strong>GET /api/Employees?apiKey=crud-web-app-key-2024</strong> - All employees</li>
                <li><strong>GET /api/Locations?apiKey=crud-web-app-key-2024</strong> - All locations</li>
                <li><strong>GET /api/Departments?apiKey=crud-web-app-key-2024</strong> - All departments</li>
            </ul>
            
            <h2>?? API Keys:</h2>
            <ul>
                <li><code>maui-app-key-2024</code> - For MAUI app sync</li>
                <li><code>crud-web-app-key-2024</code> - For admin web app</li>
            </ul>
            
            <h2>?? OpenAPI Spec:</h2>
            <p><a href="/openapi/v1.json" target="_blank">View OpenAPI JSON</a></p>
            
            <h2>?? Quick Test:</h2>
            <p><a href="/health" target="_blank">Test Health Endpoint</a></p>
        </body>
        </html>
        """, "text/html")).WithTags("Documentation");

    // --- Auto-migrate database on startup ---
    // This ensures the database is created and up-to-date when the application starts in development.
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine("? Database initialized successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? Database initialization failed: {ex.Message}");
    }
}

// --- Standard Middleware ---
app.UseHttpsRedirection(); // Redirects HTTP requests to HTTPS.
app.UseCors("McElroyPolicy"); // Applies the CORS policy defined above.

// --- Health Check Endpoint ---
// Provides a public endpoint to verify the API's operational status and database connectivity.
app.MapGet("/health", async (AppDbContext context) =>
{
    try
    {
        var canConnect = await context.Database.CanConnectAsync();
        var locationCount = canConnect ? await context.Locations.CountAsync(l => l.Active == true) : 0;

        return Results.Ok(new
        {
            Status = canConnect ? "Healthy" : "Unhealthy",
            Timestamp = DateTime.UtcNow,
            Database = canConnect ? "Connected" : "Disconnected",
            ActiveLocations = locationCount,
            Environment = app.Environment.EnvironmentName,
            Message = "McElroy Directory API is running"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Health check failed: {ex.Message}");
    }
}).WithTags("Health").WithOpenApi();

// Maps attribute-routed controllers to endpoints.
app.MapControllers();

// --- Startup Logging ---
Console.WriteLine($"?? McElroy Directory API starting...");
Console.WriteLine($"?? Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"?? Navigate to the root URL to see available endpoints");

// Run the application.
app.Run();
