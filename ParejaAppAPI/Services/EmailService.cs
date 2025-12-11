using System.Net;
using System.Net.Mail;
using ParejaAppAPI.Utils;

namespace ParejaAppAPI.Services;

public interface IEmailService
{
    Task SendInvitacionParejaAsync(string toEmail, string toName, string fromName, int parejaId, int estado);
    Task SendCustomEmailAsync(string toEmail, string subject, string htmlBody);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendInvitacionParejaAsync(string toEmail, string toName, string fromName, int parejaId, int estado)
    {
        try
        {
            var frontendUrl = _configuration["FrontendUrl"];

            var acceptUrl = $"{frontendUrl}/app/perfil?action=aceptar&pareja={parejaId}";
            var rejectUrl = $"{frontendUrl}/app/perfil?action=rechazar&pareja={parejaId}";

            var bodyContent = $@"
                <p>Hola <strong>{toName}</strong>,</p>
                <p><strong>{fromName}</strong> te ha enviado una invitación para ser su pareja en Pareja App.</p>
                <p>Acepta la invitación para comenzar a compartir citas, metas y memorias especiales juntos.</p>
                
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{acceptUrl}' style='display: inline-block; padding: 15px 40px; background: linear-gradient(135deg, #ec4899 0%, #a855f7 100%); color: white; text-decoration: none; border-radius: 50px; font-weight: bold; font-size: 16px; margin: 0 10px;'>✓ Aceptar Invitación</a>
                    <a href='{rejectUrl}' style='display: inline-block; padding: 15px 40px; background: #f3f4f6; color: #666; text-decoration: none; border-radius: 50px; font-weight: bold; font-size: 16px; margin: 0 10px;'>✗ Rechazar</a>
                </div>
                
                <p style='font-size: 14px; color: #9ca3af; text-align: center; margin-top: 40px;'>
                    También puedes responder desde tu perfil en la aplicación
                </p>";

            var htmlBody = Utilerias.BuildEmailFromTemplate(
                $"{fromName} te invita a ser su pareja en Pareja App",
                bodyContent,
                null
            );

            string subject = "¡Nueva Invitación de Pareja!";

            await SendCustomEmailAsync(toEmail,subject,htmlBody);

            _logger.LogInformation($"Email de invitación enviado a {toEmail}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al enviar email a {toEmail}");
            throw;
        }
    }

    public async Task SendCustomEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var smtpHost = _configuration["Smtp:Host"];
            var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
            var smtpUsername = _configuration["Smtp:Username"];
            var smtpPassword = _configuration["Smtp:Password"];
            var fromEmail = _configuration["Smtp:FromEmail"];
            var fromNameConfig = _configuration["Smtp:FromName"];

            using var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail ?? smtpUsername ?? "", fromNameConfig),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation($"Email enviado a {toEmail} con asunto '{subject}'");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al enviar email a {toEmail}");
            throw;
        }
    }

}
