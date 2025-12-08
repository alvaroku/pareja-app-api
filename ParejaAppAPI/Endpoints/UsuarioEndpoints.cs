using Microsoft.AspNetCore.Mvc;
using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Services.Interfaces;

namespace ParejaAppAPI.Endpoints;

public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/usuarios").WithTags("Usuarios").RequireAuthorization();

        group.MapGet("/{id:int}", async (int id, IUsuarioService service) =>
        {
            var response = await service.GetByIdAsync(id);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapGet("/", async ([FromQuery] int pageNumber, [FromQuery] int pageSize, IUsuarioService service) =>
        {
            var response = await service.GetPagedAsync(pageNumber, pageSize);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapPut("/{id:int}", async (int id, [FromBody] UpdateUsuarioDto dto, IUsuarioService service) =>
        {
            var response = await service.UpdateAsync(id, dto);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapDelete("/{id:int}", async (int id, IUsuarioService service) =>
        {
            var response = await service.DeleteAsync(id);
            return Results.Json(response, statusCode: response.StatusCode);
        });
    }
}
