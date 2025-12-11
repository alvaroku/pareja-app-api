using ParejaAppAPI.Models.DTOs;

namespace ParejaAppAPI.Services.Interfaces
{
    public interface ISMSService
    {
        Task Send(SendSMSRequest notification);
    }
}
