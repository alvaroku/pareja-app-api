namespace ParejaAppAPI.Models.Entities
{
    public class Notification : BaseEntity
    {
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public Usuario User { get; set; } = null!;
        public bool SendImmediately { get; set; }
        public DateTime? ScheduledAtUtc { get; set; }
        public DateTime? SentAtUtc { get; set; }
        public Dictionary<string, string>? AdditionalData { get; set; }
    }
}
