// Required using statements for Entity Framework, application contexts, services, and web components
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MMPD.Data.Context;
using MMPD.Data.Data;
using MMPD.Shared.Services;
using MMPD.Web;
using MMPD.Web.Components;
using MMPD.Web.Components.Account;
using MMPD.Web.Services;

// Create the web application builder with configuration from appsettings.json and command line args
var builder = WebApplication.CreateBuilder(args);

// =============================================================================
// SERVICE REGISTRATION - Configure dependency injection container
// =============================================================================

// Configure Blazor components with interactive server-side rendering
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Register Telerik UI for Blazor component library
builder.Services.AddTelerikBlazor();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configure Entity Framework database context with SQLite
// Database file is located in the MMPD.Data project folder
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source=../MMPD.Data/Data/mcelroy_directory.db");
});

builder.Services.AddAuthentication()
    .AddCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/error";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(120);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Administrator", policy => policy.RequireRole("Administrator"))
    .AddPolicy("User", policy => policy.RequireRole("User"))
    .AddPolicy("Viewer", policy => policy.RequireRole("Viewer"))
    .AddPolicy("UserOrAbove", policy => policy.RequireRole("User", "Administrator"))
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Administrator"));

//builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
//{
//    options.SignIn.RequireConfirmedAccount = false;
//    options.Password.RequireNonAlphanumeric = false;
//    options.Password.RequireUppercase = false;
//    options.Password.RequireLowercase = false;
//    options.Password.RequiredLength = 6;

//    options.SignIn.RequireConfirmedEmail = false;
//    options.User.RequireUniqueEmail = true;
//    options.Lockout.MaxFailedAccessAttempts = 5;
//})
//    .AddEntityFrameworkStores<AppDbContext>()
//    .AddDefaultTokenProviders();

builder.Services.AddScoped<IAuthService, AuthService>();

// Register application-specific services for dependency injection
builder.Services.AddScoped<IDirectoryService, DirectoryService>(); // Directory management service
builder.Services.AddScoped<ExportData>(); // Data export functionality

// Register device/platform-specific services for responsive design
// Singleton lifetime ensures same instance across the application
builder.Services.AddSingleton<IFormFactor, FormFactor>();


builder.Services.AddHttpContextAccessor();

// Build the configured application
var app = builder.Build();

// =============================================================================
// MIDDLEWARE PIPELINE - Configure HTTP request processing pipeline
// =============================================================================

// Development-specific middleware
if (app.Environment.IsDevelopment())
{
    // Enable debugging for WebAssembly components (if used)
    app.UseWebAssemblyDebugging();
}
else
{
    // Production error handling - redirect errors to /Error page
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

    // HTTP Strict Transport Security - enforces HTTPS for 30 days
    // Consider adjusting timeframe for production needs
    app.UseHsts();
}

// Security and routing middleware (order matters!)
app.UseHttpsRedirection();    // Redirect HTTP requests to HTTPS
app.UseStaticFiles();         // Serve static files (CSS, JS, images, etc.)
app.UseAntiforgery();         // CSRF protection for forms
app.MapStaticAssets();        // Map static assets for optimization
app.UseAuthentication(); // Enable authentication
app.UseAuthorization();  // Enable authorization

// Configure Blazor component routing with server-side rendering
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

// Start the web application and begin listening for requests
app.Run();