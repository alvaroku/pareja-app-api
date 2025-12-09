using System.Net;
using System.Net.Mail;

namespace ParejaAppAPI.Services;

public interface IEmailService
{
    Task SendInvitacionParejaAsync(string toEmail, string toName, string fromName, int parejaId, int estado);
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
            var smtpHost = _configuration["Smtp:Host"];
            var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
            var smtpUsername = _configuration["Smtp:Username"];
            var smtpPassword = _configuration["Smtp:Password"];
            var fromEmail = _configuration["Smtp:FromEmail"];
            var fromNameConfig = _configuration["Smtp:FromName"];
            var frontendUrl = _configuration["FrontendUrl"];

            var acceptUrl = $"{frontendUrl}/app/perfil?action=aceptar&pareja={parejaId}";
            var rejectUrl = $"{frontendUrl}/app/perfil?action=rechazar&pareja={parejaId}";

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            margin: 0;
            padding: 20px;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background: white;
            border-radius: 20px;
            overflow: hidden;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 40px 20px;
            text-align: center;
            color: white;
        }}
        .header h1 {{
            margin: 0;
            font-size: 32px;
            font-weight: bold;
        }}
        .header p {{
            margin: 10px 0 0 0;
            font-size: 16px;
            opacity: 0.9;
        }}
        .content {{
            padding: 40px 30px;
            text-align: center;
        }}
        .content h2 {{
            color: #333;
            font-size: 24px;
            margin: 0 0 20px 0;
        }}
        .content p {{
            color: #666;
            font-size: 16px;
            line-height: 1.6;
            margin: 0 0 30px 0;
        }}
        .buttons {{
            display: flex;
            gap: 15px;
            justify-content: center;
            margin: 30px 0;
        }}
        .btn {{
            display: inline-block;
            padding: 15px 40px;
            text-decoration: none;
            border-radius: 50px;
            font-weight: bold;
            font-size: 16px;
            transition: transform 0.2s;
        }}
        .btn:hover {{
            transform: translateY(-2px);
        }}
        .btn-accept {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
        }}
        .btn-reject {{
            background: #f3f4f6;
            color: #666;
        }}
        .footer {{
            background: #f9fafb;
            padding: 20px;
            text-align: center;
            color: #999;
            font-size: 14px;
        }}
        .icon {{
            font-size: 60px;
            margin-bottom: 20px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>💑 Pareja App</h1>
            <p>Comparte momentos inolvidables juntos</p>
        </div>
        <div class='content'>
            <div class='icon'>💌</div>
            <h2>¡Nueva Invitación de Pareja!</h2>
            <p>Hola <strong>{toName}</strong>,</p>
            <p><strong>{fromName}</strong> te ha enviado una invitación para ser su pareja en Pareja App.</p>
            <p>Acepta la invitación para comenzar a compartir citas, metas y memorias especiales juntos.</p>
            
            <div class='buttons'>
                <a href='{acceptUrl}' class='btn btn-accept'>✓ Aceptar Invitación</a>
                <a href='{rejectUrl}' class='btn btn-reject'>✗ Rechazar</a>
            </div>
            
            <p style='font-size: 14px; color: #999; margin-top: 40px;'>
                También puedes responder desde tu perfil en la aplicación
            </p>
        </div>
        <div class='footer'>
            <p>© 2025 Pareja App - Hecho con ❤️</p>
            <p>Si no solicitaste esta invitación, puedes ignorar este correo.</p>
        </div>
    </div>
</body>
</html>";

            using var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail ?? smtpUsername ?? "", fromNameConfig),
                Subject = $"💑 {fromName} te invita a ser su pareja en Pareja App",
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation($"Email de invitación enviado a {toEmail}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al enviar email a {toEmail}");
            throw;
        }
    }
}
