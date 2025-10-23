// Required using statements for Entity Framework, application contexts, services, and web components
using Auth0.AspNetCore.Authentication;
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
//builder.Services.AddRazorComponents()
//    .AddInteractiveServerComponents();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register Telerik UI for Blazor component library
builder.Services.AddTelerikBlazor();

builder.Services.AddScoped<IAuthService, AuthService>();

//builder.Services.AddCascadingAuthenticationState();
//builder.Services.AddScoped<IdentityUserAccessor>();
//builder.Services.AddScoped<IdentityRedirectManager>();
//builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = IdentityConstants.ApplicationScheme;
//    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
//})
//    .AddIdentityCookies();

//builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//Trying to get Auth working here

//builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    //.AddEntityFrameworkStores<ApplicationDbContext>()
//    .AddEntityFrameworkStores<AppDbContext>()
//    .AddSignInManager()
//    .AddDefaultTokenProviders();

//builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Configure Entity Framework database context with SQLite
// Database file is located in the MMPD.Data project folder
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source=../MMPD.Data/Data/mcelroy_directory.db");
});

// Register application-specific services for dependency injection
builder.Services.AddScoped<IDirectoryService, DirectoryService>(); // Directory management service
builder.Services.AddScoped<ExportData>(); // Data export functionality

// Register device/platform-specific services for responsive design
// Singleton lifetime ensures same instance across the application
builder.Services.AddSingleton<IFormFactor, FormFactor>();

//builder.Services.AddAuth0WebAppAuthentication(options =>
//{
//    options.Domain = builder.Configuration["Auth0:Domain"];
//    options.ClientId = builder.Configuration["Auth0:ClientId"];
//});

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
//app.UseAuthentication(); // Enable authentication
//app.UseAuthorization();  // Enable authorization

// Configure Blazor component routing with server-side rendering
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
//app.MapAdditionalIdentityEndpoints();

// Start the web application and begin listening for requests
app.Run();