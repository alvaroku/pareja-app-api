using ParejaAppAPI.Models.DTOs;

namespace ParejaAppAPI.Services.Interfaces
{
    public interface IPushNotificationService
    {
        Task Send(SendNotificationRequest notification);
    }
}
