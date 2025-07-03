using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MMPD.Shared.Services;
using MMPD.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add device-specific services used by the MMPD.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

await builder.Build().RunAsync();
