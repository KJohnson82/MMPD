using Microsoft.EntityFrameworkCore;
using MMPD.Data.Context;
using MMPD.Data.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
namespace MMPD.Api.Controllers;

public static class LocationEndpoints
{
    public static void MapLocationEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Location").WithTags(nameof(Location));

        group.MapGet("/", async (AppDbContext db) =>
        {
            return await db.Locations.ToListAsync();
        })
        .WithName("GetAllLocations")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<Location>, NotFound>> (int id, AppDbContext db) =>
        {
            return await db.Locations.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is Location model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetLocationById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, Location location, AppDbContext db) =>
        {
            var affected = await db.Locations
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.Id, location.Id)
                    .SetProperty(m => m.LocName, location.LocName)
                    .SetProperty(m => m.LocNum, location.LocNum)
                    .SetProperty(m => m.Address, location.Address)
                    .SetProperty(m => m.City, location.City)
                    .SetProperty(m => m.State, location.State)
                    .SetProperty(m => m.Zipcode, location.Zipcode)
                    .SetProperty(m => m.PhoneNumber, location.PhoneNumber)
                    .SetProperty(m => m.FaxNumber, location.FaxNumber)
                    .SetProperty(m => m.Email, location.Email)
                    .SetProperty(m => m.Hours, location.Hours)
                    .SetProperty(m => m.Loctype, location.Loctype)
                    .SetProperty(m => m.AreaManager, location.AreaManager)
                    .SetProperty(m => m.StoreManager, location.StoreManager)
                    .SetProperty(m => m.RecordAdd, location.RecordAdd)
                    .SetProperty(m => m.Active, location.Active)
                    );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateLocation")
        .WithOpenApi();

        group.MapPost("/", async (Location location, AppDbContext db) =>
        {
            db.Locations.Add(location);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Location/{location.Id}",location);
        })
        .WithName("CreateLocation")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, AppDbContext db) =>
        {
            var affected = await db.Locations
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteLocation")
        .WithOpenApi();
    }
}
