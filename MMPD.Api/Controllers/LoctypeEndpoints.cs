using Microsoft.EntityFrameworkCore;
using MMPD.Data.Context;
using MMPD.Data.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
namespace MMPD.Api.Controllers;

public static class LoctypeEndpoints
{
    public static void MapLoctypeEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Loctype").WithTags(nameof(Loctype));

        group.MapGet("/", async (AppDbContext db) =>
        {
            return await db.Loctypes.ToListAsync();
        })
        .WithName("GetAllLoctypes")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<Loctype>, NotFound>> (int id, AppDbContext db) =>
        {
            return await db.Loctypes.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is Loctype model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetLoctypeById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, Loctype loctype, AppDbContext db) =>
        {
            var affected = await db.Loctypes
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.Id, loctype.Id)
                    .SetProperty(m => m.LoctypeName, loctype.LoctypeName)
                    );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateLoctype")
        .WithOpenApi();

        group.MapPost("/", async (Loctype loctype, AppDbContext db) =>
        {
            db.Loctypes.Add(loctype);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Loctype/{loctype.Id}",loctype);
        })
        .WithName("CreateLoctype")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, AppDbContext db) =>
        {
            var affected = await db.Loctypes
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteLoctype")
        .WithOpenApi();
    }
}
