using Microsoft.EntityFrameworkCore;
using MMPD.Data.Context;
using MMPD.Data.Data;

var builder = WebApplication.CreateBuilder(args);

//  CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("McElroyPolicy", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7141",
                "https://localhost:5067",
                "http://localhost:5067"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

//  Database Configuration
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlite(connectionString);

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Register Services
builder.Services.AddScoped<IDirectoryService, DirectoryService>();
builder.Services.AddScoped<ExportData>();

//  API Configuration with .NET 9 OpenAPI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

//  Development Configuration
if (app.Environment.IsDevelopment())
{
    // Use .NET 9's built-in OpenAPI
    app.MapOpenApi();

    // Add a simple endpoint to view the API documentation
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

    // Auto-migrate database
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

app.UseHttpsRedirection();
app.UseCors("McElroyPolicy");

//  Health Check Endpoint
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

app.MapControllers();

Console.WriteLine($"?? McElroy Directory API starting...");
Console.WriteLine($"?? Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"?? Navigate to the root URL to see available endpoints");

app.Run();