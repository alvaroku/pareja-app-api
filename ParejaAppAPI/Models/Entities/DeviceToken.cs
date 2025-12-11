namespace ParejaAppAPI.Models.Entities
{
    public class DeviceToken : BaseEntity
    {
        public string Token { get; set; } = string.Empty;
        public string DeviceInfo { get; set; } = string.Empty;
        public int UserId { get; set; }
        public Usuario User { get; set; }
    }
}
