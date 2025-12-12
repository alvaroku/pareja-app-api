namespace ParejaAppAPI.Models.Entities;

public class Usuario : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? CodigoPais { get; set; }
    public string? Telefono { get; set; }
    public int? ProfilePhotoId { get; set; }
    public string? TimeZone { get; set; }
    public UserRole Role { get; set; } = UserRole.User;

    // Relaciones
    public virtual Resource? ProfilePhoto { get; set; }
    public virtual ICollection<Cita> Citas { get; set; } = new List<Cita>();
    public virtual ICollection<Meta> Metas { get; set; } = new List<Meta>();
    public virtual ICollection<Memoria> Memorias { get; set; } = new List<Memoria>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<DeviceToken> DeviceTokens { get; set; } = new List<DeviceToken>();
    public virtual ICollection<RecoveryToken> RecoveryTokens { get; set; } = new List<RecoveryToken>();
}
