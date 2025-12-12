using Microsoft.AspNetCore.Mvc;
using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Models.Entities;
using ParejaAppAPI.Services.Interfaces;

namespace ParejaAppAPI.Endpoints;

public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/usuarios").WithTags("Usuarios")
         .RequireAuthorization();

        group.MapGet("/{id:int}", async (int id, IUsuarioService service) =>
        {
            var response = await service.GetByIdAsync(id);
            return Results.Json(response, statusCode: response.StatusCode);
        }).RequireAuthorization(policy => policy.RequireRole(UserRole.User.ToString(),UserRole.SuperAdmin.ToString()));

        group.MapGet("/", async ([FromQuery] int pageNumber, [FromQuery] int pageSize, IUsuarioService service) =>
        {
            var response = await service.GetPagedAsync(pageNumber, pageSize);
            return Results.Json(response, statusCode: response.StatusCode);
        }).RequireAuthorization(policy => policy.RequireRole(UserRole.SuperAdmin.ToString()));

        group.MapPost("/", async ([FromBody] CreateUsuarioDto dto, IUsuarioService service) =>
        {
            var response = await service.CreateAsync(dto);
            return Results.Json(response, statusCode: response.StatusCode);
        }).RequireAuthorization(policy => policy.RequireRole(UserRole.SuperAdmin.ToString()));

        group.MapPut("/{id:int}", async (int id, [FromBody] UpdateUsuarioDto dto, IUsuarioService service) =>
        {
            var response = await service.UpdateAsync(id, dto);
            return Results.Json(response, statusCode: response.StatusCode);
        }).RequireAuthorization(policy => policy.RequireRole(UserRole.User.ToString(), UserRole.SuperAdmin.ToString()));

        group.MapDelete("/{id:int}", async (int id, IUsuarioService service) =>
        {
            var response = await service.DeleteAsync(id);
            return Results.Json(response, statusCode: response.StatusCode);
        }).RequireAuthorization(policy => policy.RequireRole(UserRole.SuperAdmin.ToString()));

        group.MapPost("/{id:int}/upload-photo", async (int id, HttpRequest request, IResourceService resourceService) =>
        {
            if (!request.HasFormContentType)
                return Results.BadRequest(new { message = "Se requiere multipart/form-data" });

            var form = await request.ReadFormAsync();
            var file = form.Files.GetFile("file");

            if (file == null || file.Length == 0)
                return Results.BadRequest(new { message = "No se ha proporcionado ningún archivo" });

            // Validar que sea imagen
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return Results.BadRequest(new { message = "Solo se permiten archivos de imagen (jpg, jpeg, png, gif, webp)" });

            // Validar tamaño (5MB)
            if (file.Length > 5 * 1024 * 1024)
                return Results.BadRequest(new { message = "El archivo no puede superar los 5MB" });

            using var stream = file.OpenReadStream();
            var response = await resourceService.UploadForUsuarioAsync(id, stream, file.FileName, file.ContentType);
            return Results.Json(response, statusCode: response.StatusCode);
        }).RequireAuthorization(policy => policy.RequireRole(UserRole.User.ToString(), UserRole.SuperAdmin.ToString()));

        group.MapDelete("/{id:int}/delete-photo", async (int id, IUsuarioService usuarioService, IResourceService resourceService) =>
        {
            var usuarioResult = await usuarioService.GetByIdAsync(id);
            if (!usuarioResult.IsSuccess)
                return Results.Json(usuarioResult, statusCode: usuarioResult.StatusCode);

            if (usuarioResult.Data?.Resource == null)
                return Results.BadRequest(new { message = "El usuario no tiene foto de perfil" });

            var response = await resourceService.DeleteAsync(usuarioResult.Data.Resource.Id);
            return Results.Json(response, statusCode: response.StatusCode);
        }).RequireAuthorization(policy => policy.RequireRole(UserRole.User.ToString(), UserRole.SuperAdmin.ToString()));
    }
}
