using Microsoft.EntityFrameworkCore;
using ParejaAppAPI.Data;
using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Models.Entities;
using ParejaAppAPI.Services.Interfaces;
using ParejaAppAPI.Utils;

namespace ParejaAppAPI.Services.BackgroundServices
{
    public class NotificationDispatcherWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationDispatcherWorker> _logger;

        private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(30);
        private readonly TimeSpan _notificationWindow = TimeSpan.FromMinutes(1);



        public NotificationDispatcherWorker(IServiceProvider serviceProvider, ILogger<NotificationDispatcherWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationDispatcherWorker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in NotificationDispatcherWorker");
                }

                await Task.Delay(_pollInterval, stoppingToken);
            }

            _logger.LogInformation("NotificationDispatcherWorker stopped");
        }

        private async Task ProcessAsync(CancellationToken cancellationToken)
        {
            var nowUtc = DateTime.UtcNow;

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var pushService = scope.ServiceProvider.GetService<IPushNotificationService>();
            var emailService = scope.ServiceProvider.GetService<IEmailService>();
            var smsService = scope.ServiceProvider.GetService<ISMSService>();

            // immediate notifications
            var immediate = await db.Notifications
               .Include(n => n.User).ThenInclude(u => u.DeviceTokens)
                .Where(n => n.SendImmediately && n.SentAtUtc == null)
                .ToListAsync(cancellationToken);

            foreach (var n in immediate)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await TrySendNotificationAsync(n, pushService, emailService, smsService, db, nowUtc, cancellationToken);
            }

            // scheduled notifications whose time has arrived (consider small window)
            var scheduled = await db.Notifications
                .Include(n => n.User).ThenInclude(u => u.DeviceTokens)
                .Where(n =>
                    !n.SendImmediately &&
                    n.ScheduledAtUtc != null &&
                    n.SentAtUtc == null &&
                    n.ScheduledAtUtc <= nowUtc &&                                     // scheduled time arrived
                    n.ScheduledAtUtc > nowUtc - _notificationWindow                    // AND not too far in the past
                )
                .ToListAsync(cancellationToken);


            foreach (var n in scheduled)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await TrySendNotificationAsync(n, pushService, emailService, smsService, db, nowUtc, cancellationToken);
            }

            // late scheduled notifications (beyond window)
            var scheduledLate = await db.Notifications
               .Include(n => n.User)
               .Include(n => n.User).ThenInclude(u => u.DeviceTokens).Where(n =>
               !n.SendImmediately &&
               n.ScheduledAtUtc != null &&
               n.SentAtUtc == null &&
               n.ScheduledAtUtc <= nowUtc).ToListAsync(cancellationToken);

            foreach (var n in scheduledLate)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await TrySendNotificationAsync(n, pushService, emailService, smsService, db, nowUtc, cancellationToken);
            }
        }

        private async Task TrySendNotificationAsync(Notification n, IPushNotificationService? pushService, IEmailService? emailService, ISMSService? smsService, AppDbContext db, DateTime nowUtc, CancellationToken cancellationToken)
        {
            if (pushService == null)
            {
                _logger.LogWarning("No IPushNotificationService available; skipping push send for notification {NotificationId}", n.Id);
                return;
            }

            if (emailService == null)
            {
                _logger.LogWarning("No IEmailSender available; skipping push send for notification {NotificationId}", n.Id);
                return;
            }

            if (smsService == null)
            {
                _logger.LogWarning("No ISMSService available; skipping SMS send for notification {NotificationId}", n.Id);
                return;
            }

            // Convert ScheduledAtUtc to business local time for logging/analytics (reuse conversion logic)
            string tzDisplay = "UTC";
            if (!string.IsNullOrWhiteSpace(n.User.TimeZone) && n.ScheduledAtUtc.HasValue)
            {
                try
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(n.User.TimeZone);
                    var local = TimeZoneInfo.ConvertTimeFromUtc(n.ScheduledAtUtc.Value, tz);
                    tzDisplay = tz.DisplayName;
                    _logger.LogDebug("Notification {NotificationId} scheduled local time {LocalTime} ({TimeZone})", n.Id, local, tzDisplay);
                }
                catch
                {
                    tzDisplay = "UTC";
                }
            }

            try
            {
                // send push notification to all device tokens
                foreach (var item in n.User.DeviceTokens)
                {
                    var req = new SendNotificationRequest(
                        item.Token,
                        n.Title,
                        n.Body,
                        n.AdditionalData
                    );
                    await pushService.Send(req);
                }

                // send email
                var subject = n.Title;
                var to = n.User.Email;
                var htmlBody = Utilerias.BuildEmailFromTemplate(n.Title, n.Body, n.AdditionalData);

                await emailService.SendCustomEmailAsync(to, subject, htmlBody);

                // send SMS
                if (!string.IsNullOrWhiteSpace(n.User.Telefono) && !string.IsNullOrWhiteSpace(n.User.CodigoPais))
                    await smsService.Send(new SendSMSRequest(PhoneTo: $"{n.User.CodigoPais}{n.User.Telefono}", Message: $"{n.Title}\n{n.Body}"));

                n.SentAtUtc = nowUtc;
                db.Notifications.Update(n);
                await db.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Sent notification {NotificationId} to user {UserId}", n.Id, n.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification {NotificationId} to user {UserId}", n.Id, n.UserId);
            }
        }
    }
}