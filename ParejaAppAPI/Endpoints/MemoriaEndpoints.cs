using Microsoft.AspNetCore.Mvc;
using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Models.Entities;
using ParejaAppAPI.Services.Interfaces;
using System.Security.Claims;

namespace ParejaAppAPI.Endpoints;

public static class MemoriaEndpoints
{
    public static void MapMemoriaEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/memorias").WithTags("Memorias")
         .RequireAuthorization(policy => policy.RequireRole(UserRole.User.ToString(), UserRole.SuperAdmin.ToString()));;

        group.MapGet("/{id:int}", async (int id, IMemoriaService service) =>
        {
            var response = await service.GetByIdAsync(id);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapGet("/usuario/{usuarioId:int}", async (int usuarioId, IMemoriaService service) =>
        {
            var response = await service.GetByUsuarioIdAsync(usuarioId);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapGet("/mis-memorias", async (ClaimsPrincipal user, IMemoriaService service) =>
        {
            var usuarioIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out var usuarioId))
                return Results.Unauthorized();

            var response = await service.GetByUsuarioYParejaAsync(usuarioId);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapGet("/", async ([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] int? usuarioId, IMemoriaService service) =>
        {
            var response = await service.GetPagedAsync(pageNumber, pageSize, usuarioId);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapPost("/", async ([FromBody] CreateMemoriaDto dto, IMemoriaService service) =>
        {
            var response = await service.CreateAsync(dto);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapPost("/{id:int}/upload", async (int id, HttpRequest request, IResourceService resourceService) =>
        {
            if (!request.HasFormContentType)
                return Results.BadRequest("Debe enviar multipart/form-data");

            var form = await request.ReadFormAsync();
            var file = form.Files["file"];
            if (file == null || file.Length == 0)
                return Results.BadRequest("No se envió archivo o está vacío");

            using var stream = file.OpenReadStream();
            var response = await resourceService.UploadForMemoriaAsync(id, stream, file.FileName, file.ContentType);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapPut("/{id:int}", async (int id, [FromBody] UpdateMemoriaDto dto, IMemoriaService service) =>
        {
            var response = await service.UpdateAsync(id, dto);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapDelete("/{id:int}", async (int id, IMemoriaService service) =>
        {
            var response = await service.DeleteAsync(id);
            return Results.Json(response, statusCode: response.StatusCode);
        });
    }
}
