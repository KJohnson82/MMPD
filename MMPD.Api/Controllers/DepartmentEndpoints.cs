using Microsoft.EntityFrameworkCore;
using MMPD.Data.Context;
using MMPD.Data.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
namespace MMPD.Api.Controllers;

public static class DepartmentEndpoints
{
    public static void MapDepartmentEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Department").WithTags(nameof(Department));

        group.MapGet("/", async (AppDbContext db) =>
        {
            return await db.Departments.ToListAsync();
        })
        .WithName("GetAllDepartments")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<Department>, NotFound>> (int id, AppDbContext db) =>
        {
            return await db.Departments.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is Department model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetDepartmentById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, Department department, AppDbContext db) =>
        {
            var affected = await db.Departments
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.Id, department.Id)
                    .SetProperty(m => m.DeptName, department.DeptName)
                    .SetProperty(m => m.Location, department.Location)
                    .SetProperty(m => m.DeptManager, department.DeptManager)
                    .SetProperty(m => m.DeptPhone, department.DeptPhone)
                    .SetProperty(m => m.DeptEmail, department.DeptEmail)
                    .SetProperty(m => m.DeptFax, department.DeptFax)
                    .SetProperty(m => m.RecordAdd, department.RecordAdd)
                    .SetProperty(m => m.Active, department.Active)
                    );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateDepartment")
        .WithOpenApi();

        group.MapPost("/", async (Department department, AppDbContext db) =>
        {
            db.Departments.Add(department);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Department/{department.Id}",department);
        })
        .WithName("CreateDepartment")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, AppDbContext db) =>
        {
            var affected = await db.Departments
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteDepartment")
        .WithOpenApi();
    }
}
