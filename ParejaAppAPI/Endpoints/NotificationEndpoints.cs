using Microsoft.AspNetCore.Mvc;
using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Models.Entities;
using ParejaAppAPI.Services.Interfaces;
using System.Security.Claims;

namespace ParejaAppAPI.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/notifications").WithTags("Notifications").RequireAuthorization();

        group.MapPost("/", async ([FromBody] NotificationRequest dto, INotificationService service) =>
        {
            var response = await service.Create(dto);
            return Results.Json(response, statusCode: response.StatusCode);
        }).RequireAuthorization(policy => policy.RequireRole(UserRole.SuperAdmin.ToString()));;

        group.MapGet("user/{userId:int}", async (int userId, INotificationService service) =>
        {
            var response = await service.GetUserNotifications(userId, new DefaultFilterParams { });
            return Results.Json(response, statusCode: response.StatusCode);
        });


        group.MapPut("{id:int}/markAsRead", async (int id, [FromQuery] int userId, INotificationService service) =>
        {
            var response = await service.MarkAsRead(id, userId);
            return Results.Json(response, statusCode: response.StatusCode);
        });
    }
}
