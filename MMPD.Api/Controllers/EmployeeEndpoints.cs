using Microsoft.EntityFrameworkCore;
using MMPD.Data.Context;
using MMPD.Data.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
namespace MMPD.Api.Controllers;

public static class EmployeeEndpoints
{
    public static void MapEmployeeEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Employee").WithTags(nameof(Employee));

        group.MapGet("/", async (AppDbContext db) =>
        {
            return await db.Employees.ToListAsync();
        })
        .WithName("GetAllEmployees")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<Employee>, NotFound>> (int id, AppDbContext db) =>
        {
            return await db.Employees.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is Employee model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetEmployeeById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, Employee employee, AppDbContext db) =>
        {
            var affected = await db.Employees
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.Id, employee.Id)
                    .SetProperty(m => m.FirstName, employee.FirstName)
                    .SetProperty(m => m.LastName, employee.LastName)
                    .SetProperty(m => m.JobTitle, employee.JobTitle)
                    .SetProperty(m => m.IsManager, employee.IsManager)
                    .SetProperty(m => m.PhoneNumber, employee.PhoneNumber)
                    .SetProperty(m => m.CellNumber, employee.CellNumber)
                    .SetProperty(m => m.Extension, employee.Extension)
                    .SetProperty(m => m.Email, employee.Email)
                    .SetProperty(m => m.NetworkId, employee.NetworkId)
                    .SetProperty(m => m.EmpAvatar, employee.EmpAvatar)
                    .SetProperty(m => m.Location, employee.Location)
                    .SetProperty(m => m.Department, employee.Department)
                    .SetProperty(m => m.RecordAdd, employee.RecordAdd)
                    .SetProperty(m => m.Active, employee.Active)
                    );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateEmployee")
        .WithOpenApi();

        group.MapPost("/", async (Employee employee, AppDbContext db) =>
        {
            db.Employees.Add(employee);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Employee/{employee.Id}",employee);
        })
        .WithName("CreateEmployee")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, AppDbContext db) =>
        {
            var affected = await db.Employees
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteEmployee")
        .WithOpenApi();
    }
}
