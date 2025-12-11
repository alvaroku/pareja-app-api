namespace ParejaAppAPI.Utils
{
    public static class Utilerias
    {
        public const string EMAIL_TEMPLATE = @"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{{SUBJECT}}</title>
    <style>
        body { margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f3f4f6; }
        .email-container { max-width: 600px; margin: 0 auto; background-color: #ffffff; }
        .header { background: linear-gradient(135deg, #ec4899 0%, #a855f7 100%); padding: 40px 20px; text-align: center; }
        .header-icon { width: 60px; height: 60px; background-color: rgba(255, 255, 255, 0.2); border-radius: 20px; display: inline-flex; align-items: center; justify-content: center; margin-bottom: 16px; }
        .header-icon img { border-radius: 20px; width: 100%; height: 100%; object-fit: cover; }
        .header-title { color: #ffffff; font-size: 28px; font-weight: bold; margin: 0; text-shadow: 0 2px 4px rgba(0, 0, 0, 0.1); }
        .content { padding: 40px 30px; color: #1f2937; line-height: 1.6; }
        .content h2 { color: #ec4899; font-size: 24px; margin-top: 0; margin-bottom: 20px; }
        .content p { font-size: 16px; color: #4b5563; margin: 16px 0; }
        .additional-data { background-color: #fdf2f8; border-left: 4px solid #ec4899; padding: 16px; margin: 20px 0; border-radius: 8px; }
        .additional-data-item { margin: 8px 0; font-size: 14px; }
        .additional-data-key { font-weight: 600; color: #ec4899; }
        .additional-data-value { color: #4b5563; }
        .footer { background-color: #f9fafb; padding: 30px 20px; text-align: center; border-top: 1px solid #e5e7eb; }
        .footer-text { color: #6b7280; font-size: 14px; margin: 8px 0; }
        .footer-brand { background: linear-gradient(135deg, #ec4899 0%, #a855f7 100%); -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text; font-weight: bold; font-size: 18px; margin: 16px 0 8px 0; }
        .divider { height: 1px; background: linear-gradient(90deg, transparent, #e5e7eb, transparent); margin: 20px 0; }
        @media only screen and (max-width: 600px) { .content { padding: 30px 20px; } .header { padding: 30px 20px; } }
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            <div class='header-icon'>
               <img src='https://parejas-app.web.app/assets/icons/icon-192-maskable.png' />
            </div>
            <h1 class='header-title'>Pareja App</h1>
        </div>
        <div class='content'>
            <h2>{{TITLE}}</h2>
            <div class='divider'></div>
            {{BODY}}
            {{ADDITIONAL_DATA}}
        </div>
        <div class='footer'>
            <p class='footer-brand'>Pareja App</p>
            <p class='footer-text'>Comparte momentos especiales con quien más quieres</p>
            <p class='footer-text' style='margin-top: 20px;'>Este correo fue enviado automáticamente. Por favor no respondas a este mensaje.</p>
            <p class='footer-text' style='font-size: 12px; color: #9ca3af;'>© {{YEAR}} Pareja App. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
        public static string BuildEmailFromTemplate(string title, string body, Dictionary<string, string>? additionalData)
        {
            var template = EMAIL_TEMPLATE;

            // Replace placeholders
            template = template.Replace("{{SUBJECT}}", title);
            template = template.Replace("{{TITLE}}", title);

            // Format body with proper HTML paragraphs
            var formattedBody = string.Join("", body.Split('\n').Select(line =>
                string.IsNullOrWhiteSpace(line) ? "" : $"<p>{line}</p>"));
            template = template.Replace("{{BODY}}", formattedBody);

            // Build additional data section
            var additionalDataHtml = "";
            if (additionalData != null && additionalData.Any())
            {
                var items = string.Join("", additionalData.Select(kvp =>
                    $"<div class='additional-data-item'><span class='additional-data-key'>{kvp.Key}:</span> <span class='additional-data-value'>{kvp.Value}</span></div>"));

                additionalDataHtml = $"<div class='additional-data'>{items}</div>";
            }
            template = template.Replace("{{ADDITIONAL_DATA}}", additionalDataHtml);

            // Replace year
            template = template.Replace("{{YEAR}}", DateTime.Now.Year.ToString());

            return template;
        }
    }
}
