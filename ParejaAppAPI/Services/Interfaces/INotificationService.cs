using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Models.Responses;

namespace ParejaAppAPI.Services.Interfaces
{
    public interface INotificationService
    {
        Task<Response<bool>> Create(NotificationRequest notification);
        Task<Response<bool>> MarkAsRead(int notificationId, int userId);
        Task<Response<IEnumerable<NotificationResponse>>> GetUserNotifications(int userId, DefaultFilterParams filter);
    }
}
