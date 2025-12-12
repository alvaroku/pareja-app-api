namespace ParejaAppAPI.Models.Entities;

public class RecoveryToken : BaseEntity
{
    public Guid Token { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
}
