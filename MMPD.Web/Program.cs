using Microsoft.EntityFrameworkCore;
using MMPD.Data.Context;
using MMPD.Shared.Services;
using MMPD.Web;
using MMPD.Web.Components;
using MMPD.Web.Services;
using MMPD.Data.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
    //.AddInteractiveWebAssemblyComponents();

builder.Services.AddTelerikBlazor();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source=../MMPD.Data/Data/mcelroy_directory.db");
});

builder.Services.AddScoped<IDirectoryService, DirectoryService>();

builder.Services.AddScoped<ExportData>();

// Add device-specific services used by the MMPD.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
    

app.Run();
