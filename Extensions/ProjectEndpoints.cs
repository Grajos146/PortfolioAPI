using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioAPI.Data;
using PortfolioAPI.Dtos;

namespace PortfolioAPI.Extensions;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this WebApplication app)
    {
        app.MapGet(
            "/api/projects",
            async (ApplicationDbContext db) =>
            {
                var projects = await db
                    .Projects.OrderByDescending(p => p.CreatedAt)
                    .Select(p => p.ToResponseDto())
                    .ToListAsync();

                return Results.Ok(projects);
            }
        );

        app.MapGet(
            "/api/projects/{id}",
            async (int id, ApplicationDbContext db) =>
            {
                var project = await db.Projects.FindAsync(id);
                if (project == null)
                    return Results.NotFound();

                return Results.Ok(project.ToResponseDto());
            }
        );

        app.MapPost(
            "/api/projects",
            async (
                CreateProjectDto dto,
                ApplicationDbContext db,
                IConfiguration config,
                HttpRequest request
            ) =>
            {
                var secretKey = config["AdminSecretKey"];

                if (
                    !request.Headers.TryGetValue("X-Admin-Secret", out var providedKey)
                    || providedKey != secretKey
                )
                {
                    return Results.Unauthorized();
                }
                var project = dto.ToEntity();

                db.Projects.Add(project);
                await db.SaveChangesAsync();

                return Results.Created($"/api/projects/{project.Id}", project.ToResponseDto());
            }
        );

        app.MapPut(
            "/api/projects/{id}",
            async (
                int id,
                CreateProjectDto dto,
                ApplicationDbContext db,
                IConfiguration config,
                HttpRequest request
            ) =>
            {
                var secretKey = config["AdminSecretKey"];
                var project = await db.Projects.FindAsync(id);
                if (project == null)
                    return Results.NotFound();

                if (
                    !request.Headers.TryGetValue("X-Admin-Secret", out var providedKey)
                    || providedKey != secretKey
                )
                {
                    return Results.Unauthorized();
                }

                project.UpdateFromDto(dto);

                await db.SaveChangesAsync();
                return Results.Ok(project.ToResponseDto());
            }
        );

        app.MapDelete(
            "/api/projects/{id}",
            async (int id, ApplicationDbContext db, IConfiguration config, HttpRequest request) =>
            {
                var secretKey = config["AdminSecretKey"];
                if (
                    !request.Headers.TryGetValue("X-Admin-Secret", out var providedKey)
                    || providedKey != secretKey
                )
                {
                    return Results.Unauthorized();
                }

                var project = await db.Projects.FindAsync(id);
                if (project == null)
                    return Results.NotFound();

                db.Projects.Remove(project);
                await db.SaveChangesAsync();

                return Results.NoContent();
            }
        );
    }
}
