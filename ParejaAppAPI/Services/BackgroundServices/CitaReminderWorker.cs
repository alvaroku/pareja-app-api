using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ParejaAppAPI.Data;
using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Models.Entities;
using ParejaAppAPI.Services.Interfaces;
using ParejaAppAPI.Utils;

namespace ParejaAppAPI.Services.BackgroundServices
{
    public class CitaReminderWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CitaReminderWorker> _logger;
        private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(30);

        public CitaReminderWorker(IServiceProvider serviceProvider, ILogger<CitaReminderWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CitaReminderWorker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessCitaRemindersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing cita reminders");
                }

                await Task.Delay(_pollInterval, stoppingToken);
            }

            _logger.LogInformation("CitaReminderWorker stopped");
        }

        private async Task ProcessCitaRemindersAsync(CancellationToken cancellationToken)
        {
            var nowUtc = DateTime.UtcNow;

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var pushService = scope.ServiceProvider.GetService<IPushNotificationService>();
            var emailService = scope.ServiceProvider.GetService<IEmailService>();
            var smsService = scope.ServiceProvider.GetService<ISMSService>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            if (pushService == null || emailService == null || smsService == null)
            {
                _logger.LogWarning("One or more notification services are not available");
                return;
            }

            // Obtener citas que necesitan ser notificadas
            // La fecha de notificación es: FechaHora - MinutosAntesNotificar
            var citas = await db.Citas
                .Include(c => c.Usuario)
                    .ThenInclude(u => u.DeviceTokens)
                .Where(c =>
                    !c.NotificacionEnviada &&
                    c.FechaHora > nowUtc && // La cita no ha pasado
                    EF.Functions.DateDiffMinute(nowUtc, c.FechaHora) <= c.MinutosAntesNotificar) // Ya es hora de notificar
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} citas to notify", citas.Count);

            foreach (var cita in citas)
            {
                try
                {
                    await SendCitaReminderAsync(cita, pushService, emailService, smsService, db, configuration, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending reminder for cita {CitaId}", cita.Id);
                }
            }
        }

        private async Task SendCitaReminderAsync(
            Cita cita,
            IPushNotificationService pushService,
            IEmailService emailService,
            ISMSService smsService,
            AppDbContext db,
            IConfiguration configuration,
            CancellationToken cancellationToken)
        {
            var frontendUrl = configuration["FrontendUrl"];

            // Convertir fecha a timezone del usuario
            var fechaHoraLocal = cita.FechaHora.ToTimeZone(cita.Usuario.TimeZone);
            var fechaFormateada = fechaHoraLocal.ToString("dd/MM/yyyy HH:mm");

            var titulo = $"Recordatorio: {cita.Titulo}";
            var mensaje = $"Tu cita '{cita.Titulo}' es el {fechaFormateada}";
            if (!string.IsNullOrEmpty(cita.Lugar))
                mensaje += $" en {cita.Lugar}";

            // Enviar notificación push al usuario
            try
            {
                var tokens = cita.Usuario.DeviceTokens?.Select(dt => dt.Token).ToList();
                if (tokens != null && tokens.Any())
                {
                    await pushService.SendNotificationAsync(tokens, titulo, mensaje, new Dictionary<string, string>
                    {
                        { "type", "cita-reminder" },
                        { "citaId", cita.Id.ToString() },
                        { "url", $"{frontendUrl}/app/citas" }
                    });
                    _logger.LogInformation("Push notification sent to user {UserId} for cita {CitaId}", cita.UsuarioId, cita.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification for cita {CitaId}", cita.Id);
            }

            // Enviar email al usuario
            try
            {
                var emailBody = $@"
                    <p>Hola <strong>{cita.Usuario.Nombre}</strong>,</p>
                    <p>Este es un recordatorio de tu cita:</p>
                    <div style='background-color: #fdf2f8; border-left: 4px solid #ec4899; padding: 16px; margin: 20px 0; border-radius: 8px;'>
                        <div style='margin: 8px 0; font-size: 16px;'><span style='font-weight: 600; color: #ec4899;'>Título:</span> <span style='color: #4b5563;'>{cita.Titulo}</span></div>
                        {(!string.IsNullOrEmpty(cita.Descripcion) ? $"<div style='margin: 8px 0; font-size: 14px;'><span style='font-weight: 600; color: #ec4899;'>Descripción:</span> <span style='color: #4b5563;'>{cita.Descripcion}</span></div>" : "")}
                        <div style='margin: 8px 0; font-size: 14px;'><span style='font-weight: 600; color: #ec4899;'>Fecha y Hora:</span> <span style='color: #4b5563;'>{fechaFormateada}</span></div>
                        {(!string.IsNullOrEmpty(cita.Lugar) ? $"<div style='margin: 8px 0; font-size: 14px;'><span style='font-weight: 600; color: #ec4899;'>Lugar:</span> <span style='color: #4b5563;'>{cita.Lugar}</span></div>" : "")}
                    </div>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{frontendUrl}/app/citas' style='display: inline-block; padding: 15px 40px; background: linear-gradient(135deg, #ec4899 0%, #a855f7 100%); color: white; text-decoration: none; border-radius: 50px; font-weight: bold; font-size: 16px;'>Ver Citas</a>
                    </div>
                ";
                var htmlBody = Utilerias.BuildEmailFromTemplate(titulo, emailBody, null);
                await emailService.SendCustomEmailAsync(cita.Usuario.Email, titulo, htmlBody);
                _logger.LogInformation("Email sent to user {UserId} for cita {CitaId}", cita.UsuarioId, cita.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email for cita {CitaId}", cita.Id);
            }

            // Enviar SMS al usuario si tiene teléfono
            if (!string.IsNullOrEmpty(cita.Usuario.Telefono) && !string.IsNullOrEmpty(cita.Usuario.CodigoPais))
            {
                try
                {
                    var smsMessage = $"{titulo}. {mensaje}.";
                    await smsService.Send(new SendSMSRequest($"{cita.Usuario.CodigoPais}{cita.Usuario.Telefono}", smsMessage));
                    _logger.LogInformation("SMS sent to user {UserId} for cita {CitaId}", cita.UsuarioId, cita.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending SMS for cita {CitaId}", cita.Id);
                }
            }

            // Obtener pareja activa del usuario
            var pareja = await db.Parejas
                .Include(p => p.UsuarioEnvia)
                    .ThenInclude(u => u.DeviceTokens)
                .Include(p => p.UsuarioRecibe)
                    .ThenInclude(u => u.DeviceTokens)
                .FirstOrDefaultAsync(p =>
                    (p.UsuarioEnviaId == cita.UsuarioId || p.UsuarioRecibeId == cita.UsuarioId) &&
                    p.Estado == EstadoInvitacion.Aceptada,
                    cancellationToken);

            if (pareja != null)
            {
                var parejaUsuario = pareja.UsuarioEnviaId == cita.UsuarioId ? pareja.UsuarioRecibe : pareja.UsuarioEnvia;
                var mensajePareja = $"Recordatorio: Tu pareja {cita.Usuario.Nombre} tiene una cita '{cita.Titulo}' el {fechaFormateada}";

                // Enviar push a la pareja
                try
                {
                    var tokensPareja = parejaUsuario.DeviceTokens?.Select(dt => dt.Token).ToList();
                    if (tokensPareja != null && tokensPareja.Any())
                    {
                        await pushService.SendNotificationAsync(tokensPareja, $"Cita de {cita.Usuario.Nombre}", mensajePareja, new Dictionary<string, string>
                        {
                            { "type", "pareja-cita-reminder" },
                            { "citaId", cita.Id.ToString() },
                            { "url", $"{frontendUrl}/app/citas" }
                        });
                        _logger.LogInformation("Push notification sent to partner {UserId} for cita {CitaId}", parejaUsuario.Id, cita.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending push to partner for cita {CitaId}", cita.Id);
                }

                // Enviar email a la pareja
                try
                {
                    var fechaHoraLocalPareja = cita.FechaHora.ToTimeZone(parejaUsuario.TimeZone);
                    var fechaFormateadaPareja = fechaHoraLocalPareja.ToString("dd/MM/yyyy HH:mm");

                    var emailBodyPareja = $@"
                        <p>Hola <strong>{parejaUsuario.Nombre}</strong>,</p>
                        <p>Este es un recordatorio de la cita de tu pareja <strong>{cita.Usuario.Nombre}</strong>:</p>
                        <div style='background-color: #fdf2f8; border-left: 4px solid #ec4899; padding: 16px; margin: 20px 0; border-radius: 8px;'>
                            <div style='margin: 8px 0; font-size: 16px;'><span style='font-weight: 600; color: #ec4899;'>Título:</span> <span style='color: #4b5563;'>{cita.Titulo}</span></div>
                            {(!string.IsNullOrEmpty(cita.Descripcion) ? $"<div style='margin: 8px 0; font-size: 14px;'><span style='font-weight: 600; color: #ec4899;'>Descripción:</span> <span style='color: #4b5563;'>{cita.Descripcion}</span></div>" : "")}
                            <div style='margin: 8px 0; font-size: 14px;'><span style='font-weight: 600; color: #ec4899;'>Fecha y Hora:</span> <span style='color: #4b5563;'>{fechaFormateadaPareja}</span></div>
                            {(!string.IsNullOrEmpty(cita.Lugar) ? $"<div style='margin: 8px 0; font-size: 14px;'><span style='font-weight: 600; color: #ec4899;'>Lugar:</span> <span style='color: #4b5563;'>{cita.Lugar}</span></div>" : "")}
                        </div>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{frontendUrl}/app/citas' style='display: inline-block; padding: 15px 40px; background: linear-gradient(135deg, #ec4899 0%, #a855f7 100%); color: white; text-decoration: none; border-radius: 50px; font-weight: bold; font-size: 16px;'>Ver Citas</a>
                        </div>
                    ";
                    var htmlBodyPareja = Utilerias.BuildEmailFromTemplate($"Cita de {cita.Usuario.Nombre}", emailBodyPareja, null);
                    await emailService.SendCustomEmailAsync(parejaUsuario.Email, $"Recordatorio: Cita de {cita.Usuario.Nombre}", htmlBodyPareja);
                    _logger.LogInformation("Email sent to partner {UserId} for cita {CitaId}", parejaUsuario.Id, cita.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending email to partner for cita {CitaId}", cita.Id);
                }

                // Enviar SMS a la pareja si tiene teléfono
                if (!string.IsNullOrEmpty(parejaUsuario.Telefono) && !string.IsNullOrEmpty(parejaUsuario.CodigoPais))
                {
                    try
                    {
                        await smsService.Send(new SendSMSRequest( PhoneTo: $"{parejaUsuario.CodigoPais}{parejaUsuario.Telefono}",Message : mensajePareja) );
                        _logger.LogInformation("SMS sent to partner {UserId} for cita {CitaId}", parejaUsuario.Id, cita.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending SMS to partner for cita {CitaId}", cita.Id);
                    }
                }
            }

            // Marcar la cita como notificada
            cita.NotificacionEnviada = true;
            await db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Cita {CitaId} marked as notified", cita.Id);
        }
    }
}
