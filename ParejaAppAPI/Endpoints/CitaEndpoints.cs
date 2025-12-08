using Microsoft.AspNetCore.Mvc;
using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Services.Interfaces;

namespace ParejaAppAPI.Endpoints;

public static class CitaEndpoints
{
    public static void MapCitaEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/citas").WithTags("Citas").RequireAuthorization();

        group.MapGet("/{id:int}", async (int id, ICitaService service) =>
        {
            var response = await service.GetByIdAsync(id);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapGet("/usuario/{usuarioId:int}", async (int usuarioId, ICitaService service) =>
        {
            var response = await service.GetByUsuarioIdAsync(usuarioId);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapGet("/", async ([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] int? usuarioId, ICitaService service) =>
        {
            var response = await service.GetPagedAsync(pageNumber, pageSize, usuarioId);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapPost("/", async ([FromBody] CreateCitaDto dto, ICitaService service) =>
        {
            var response = await service.CreateAsync(dto);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapPut("/{id:int}", async (int id, [FromBody] UpdateCitaDto dto, ICitaService service) =>
        {
            var response = await service.UpdateAsync(id, dto);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapDelete("/{id:int}", async (int id, ICitaService service) =>
        {
            var response = await service.DeleteAsync(id);
            return Results.Json(response, statusCode: response.StatusCode);
        });
    }
}
