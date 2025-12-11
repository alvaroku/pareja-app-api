using Microsoft.AspNetCore.Mvc;
using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Repositories.Interfaces;
using ParejaAppAPI.Services.Interfaces;
using System.Security.Claims;

namespace ParejaAppAPI.Endpoints;

public static class CitaEndpoints
{
    public static void MapCitaEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/citas").WithTags("Citas").RequireAuthorization();

        group.MapGet("/{id:int}", async (int id, ClaimsPrincipal user, ICitaService service, IUsuarioRepository usuarioRepository) =>
        {
            var response = await service.GetByIdAsync(id, GetCurrentUserId(user));
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapGet("/usuario/{usuarioId:int}", async (int usuarioId, ICitaService service, ClaimsPrincipal user) =>
        {
            var response = await service.GetByUsuarioIdAsync(usuarioId, GetCurrentUserId(user));
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapGet("/mis-citas", async (ClaimsPrincipal user, ICitaService service) =>
        {
            var response = await service.GetByUsuarioYParejaAsync(GetCurrentUserId(user));
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapGet("/", async ([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] int? usuarioId, ClaimsPrincipal user, ICitaService service) =>
        {
            var response = await service.GetPagedAsync(pageNumber, pageSize, GetCurrentUserId(user), usuarioId);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapPost("/", async ([FromBody] CreateCitaDto dto, ClaimsPrincipal user, ICitaService service) =>
        {
            var response = await service.CreateAsync(dto, GetCurrentUserId(user));
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapPut("/{id:int}", async (int id, [FromBody] UpdateCitaDto dto, ClaimsPrincipal user, ICitaService service) =>
        {
            var response = await service.UpdateAsync(id, dto, GetCurrentUserId(user));
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapDelete("/{id:int}", async (int id, ICitaService service) =>
        {
            var response = await service.DeleteAsync(id);
            return Results.Json(response, statusCode: response.StatusCode);
        });
    }

    private static int GetCurrentUserId(ClaimsPrincipal user)
    {
        var usuarioIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out int usuarioId))
            return 0;
        return usuarioId;
    }
}
