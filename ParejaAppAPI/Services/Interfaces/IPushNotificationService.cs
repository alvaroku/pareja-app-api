using ParejaAppAPI.Models.DTOs;

namespace ParejaAppAPI.Services.Interfaces
{
    public interface IPushNotificationService
    {
        Task Send(SendNotificationRequest notification);
        Task SendNotificationAsync(List<string> tokens, string titulo, string mensaje, Dictionary<string, string> dictionary);
    }
}
