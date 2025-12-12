using Microsoft.AspNetCore.Mvc;
using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Services.Interfaces;

namespace ParejaAppAPI.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth").WithTags("Authentication");

        group.MapPost("/login", async ([FromBody] LoginDto dto, IAuthService authService) =>
        {
            var response = await authService.LoginAsync(dto);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapPost("/register", async ([FromBody] RegisterDto dto, IAuthService authService) =>
        {
            var response = await authService.RegisterAsync(dto);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapPost("/request-password-recovery", async ([FromBody] RequestPasswordRecoveryDto dto, IAuthService authService) =>
        {
            var response = await authService.RequestPasswordRecoveryAsync(dto);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapPost("/reset-password", async ([FromBody] ResetPasswordDto dto, IAuthService authService) =>
        {
            var response = await authService.ResetPasswordAsync(dto);
            return Results.Json(response, statusCode: response.StatusCode);
        });
    }
}
