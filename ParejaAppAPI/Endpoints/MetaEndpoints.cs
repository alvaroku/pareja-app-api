using Microsoft.AspNetCore.Mvc;
using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Models.Entities;
using ParejaAppAPI.Services.Interfaces;
using System.Security.Claims;

namespace ParejaAppAPI.Endpoints;

public static class MetaEndpoints
{
    public static void MapMetaEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/metas").WithTags("Metas")
         .RequireAuthorization(policy => policy.RequireRole(UserRole.User.ToString(), UserRole.SuperAdmin.ToString()));;

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

        group.MapGet("/mis-metas", async (ClaimsPrincipal user, IMetaService service) =>
        {
            var usuarioIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out int usuarioId))
                return Results.Json(Models.Responses.Response<object>.Failure(401, "Usuario no autenticado"), statusCode: 401);

            var response = await service.GetByUsuarioYParejaAsync(usuarioId);
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
