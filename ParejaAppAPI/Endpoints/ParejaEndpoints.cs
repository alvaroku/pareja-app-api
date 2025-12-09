using Microsoft.AspNetCore.Mvc;
using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Services.Interfaces;
using System.Security.Claims;

namespace ParejaAppAPI.Endpoints;

public static class ParejaEndpoints
{
    public static void MapParejaEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/pareja").WithTags("Pareja").RequireAuthorization();

        // Obtener pareja activa o invitación pendiente del usuario
        group.MapGet("/", async (ClaimsPrincipal user, IParejaService service) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Results.Unauthorized();

            var response = await service.GetParejaActivaAsync(userId);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        // Enviar invitación de pareja
        group.MapPost("/invitar", async (ClaimsPrincipal user, [FromBody] EnviarInvitacionDto dto, IParejaService service) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Results.Unauthorized();

            var response = await service.EnviarInvitacionAsync(userId, dto);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        // Responder invitación (aceptar/rechazar)
        group.MapPost("/responder", async (ClaimsPrincipal user, [FromBody] ResponderInvitacionDto dto, IParejaService service) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Results.Unauthorized();

            var response = await service.ResponderInvitacionAsync(userId, dto);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        // Eliminar pareja o cancelar invitación
        group.MapDelete("/", async (ClaimsPrincipal user, IParejaService service) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Results.Unauthorized();

            var response = await service.EliminarParejaAsync(userId);
            return Results.Json(response, statusCode: response.StatusCode);
        });
    }
}
