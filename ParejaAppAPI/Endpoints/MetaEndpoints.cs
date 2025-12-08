using Microsoft.AspNetCore.Mvc;
using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Services.Interfaces;

namespace ParejaAppAPI.Endpoints;

public static class MetaEndpoints
{
    public static void MapMetaEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/metas").WithTags("Metas").RequireAuthorization();

        group.MapGet("/{id:int}", async (int id, IMetaService service) =>
        {
            var response = await service.GetByIdAsync(id);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapGet("/usuario/{usuarioId:int}", async (int usuarioId, IMetaService service) =>
        {
            var response = await service.GetByUsuarioIdAsync(usuarioId);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapGet("/", async ([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] int? usuarioId, IMetaService service) =>
        {
            var response = await service.GetPagedAsync(pageNumber, pageSize, usuarioId);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapPost("/", async ([FromBody] CreateMetaDto dto, IMetaService service) =>
        {
            var response = await service.CreateAsync(dto);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapPut("/{id:int}", async (int id, [FromBody] UpdateMetaDto dto, IMetaService service) =>
        {
            var response = await service.UpdateAsync(id, dto);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapDelete("/{id:int}", async (int id, IMetaService service) =>
        {
            var response = await service.DeleteAsync(id);
            return Results.Json(response, statusCode: response.StatusCode);
        });
    }
}
