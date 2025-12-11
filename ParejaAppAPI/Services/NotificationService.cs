using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Models.Entities;
using ParejaAppAPI.Models.Responses;
using ParejaAppAPI.Repositories.Interfaces;
using ParejaAppAPI.Services.Interfaces;
using System.Linq.Expressions;

namespace ParejaAppAPI.Services
{
    public class NotificationService(INotificationRepository notificationRepository) : INotificationService
    {
        public async Task<Response<bool>> Create(NotificationRequest notification)
        {
            var entity = new Notification
            {
                UserId = notification.UserId,
                Title = notification.Title,
                Body = notification.Body,
                AdditionalData = notification.AdditionalData,
                SendImmediately = notification.SendImmediately,
                ScheduledAtUtc = notification.ScheduledAtUtc,
            };

            await notificationRepository.AddAsync(entity);

            var resp = new NotificationResponse
            (
                 entity.Id,
                 entity.UserId,
                 entity.Title,
                 entity.Body,
                 entity.IsRead
            );

            return Response<bool>.Success(true);
        }
        public async Task<Response<bool>> MarkAsRead(int notificationId, int userId)
        {
            if (!await notificationRepository.Exists(x => x.Id == notificationId && x.UserId == userId))
                return Response<bool>.Failure(404, "Notificación no encontrada");
            var n = await notificationRepository.GetByIdAsync(notificationId);
            n.IsRead = true;

            await notificationRepository.UpdateAsync(n);
            return Response<bool>.Success(true);
        }
        public async Task<Response<IEnumerable<NotificationResponse>>> GetUserNotifications(int userId, DefaultFilterParams filter)
        {
            // apply user filter explicitly
            Expression<Func<Notification, bool>> baseFilter = x => x.UserId == userId;

            var data = await notificationRepository.GetByUsuarioIdAsync(userId);

            var list = data.Select(x => new NotificationResponse
            (
                 x.Id,
                 x.UserId,
                 x.Title,
                 x.Body,
                 x.IsRead
            )).ToList();

            return Response<IEnumerable<NotificationResponse>>.Success(list, 200);
        }
    }
}
