using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Builder.Extensions;
using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Services.Interfaces;
using System.Text;

namespace ParejaAppAPI.Services
{
    public class FirebasePushNotificationService(IConfiguration _configuration) : IPushNotificationService
    {
        private FirebaseApp _firebaseApp;

        async Task EnssureDefaultInstance()
        {
            if (FirebaseMessaging.DefaultInstance is null)
            {
                var base64 = _configuration["Firebase:Credentials"];
                if (string.IsNullOrWhiteSpace(base64))
                    throw new InvalidOperationException("Falta Firebase:Credentials en configuración.");

                var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
                _firebaseApp = FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromJson(json)
                });
            }

        }
        async Task EnviarNotificacionAsync(string token, string titulo, string cuerpo, Dictionary<string, string>? additionalData)
        {
            await EnssureDefaultInstance();

            var message = new Message()
            {
                Token = token,
                Notification = new Notification
                {
                    Title = titulo,
                    Body = cuerpo
                },
                Data = additionalData,

                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        Sound = "default",
                        ClickAction = "FLUTTER_NOTIFICATION_CLICK",
                        ChannelId = "EVENT_REMINDER"
                    }
                },

                Apns = new ApnsConfig
                {
                    Headers = new Dictionary<string, string>
            {
                { "apns-priority", "10" }
            },
                    Aps = new Aps
                    {
                        Alert = new ApsAlert
                        {
                            Title = titulo,
                            Body = cuerpo
                        },
                        Sound = "default",
                        Category = "EVENT_REMINDER",
                        ContentAvailable = true,
                    }
                }
            };

            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }

        public async Task Send(SendNotificationRequest notification)
        {
            await EnviarNotificacionAsync(notification.Token, notification.Titulo, notification.Cuerpo, notification.AdditionalData);
        }

        public async Task SendNotificationAsync(List<string> tokens, string titulo, string mensaje, Dictionary<string, string> dictionary)
        {
            foreach (var token in tokens)
            {
                await EnviarNotificacionAsync(token, titulo, mensaje, dictionary);
            }
        }
    }
}
