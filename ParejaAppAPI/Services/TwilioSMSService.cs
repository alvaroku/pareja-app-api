using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Services.Interfaces;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ParejaAppAPI.Services
{
    public class TwilioSMSService(IConfiguration _configuration) : ISMSService
    {

        public async Task Send(SendSMSRequest notification)
        {
            var accountSid = _configuration["SMS:Twilio:AccountSid"];
            var authToken = _configuration["SMS:Twilio:AuthToken"];
            TwilioClient.Init(accountSid, authToken);
            var messageOptions = new CreateMessageOptions(
              new PhoneNumber(notification.PhoneTo));
            messageOptions.From = new PhoneNumber(_configuration["SMS:Twilio:From"]);
            messageOptions.Body = notification.Message;
            var message = MessageResource.Create(messageOptions);
        }
    }
}
