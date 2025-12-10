namespace ParejaAppAPI.Models.Entities;

public class Usuario : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? CodigoPais { get; set; }
    public string? Telefono { get; set; }
    public int? ProfilePhotoId { get; set; }

    // Relaciones
    public virtual Resource? ProfilePhoto { get; set; }
    public virtual ICollection<Cita> Citas { get; set; } = new List<Cita>();
    public virtual ICollection<Meta> Metas { get; set; } = new List<Meta>();
    public virtual ICollection<Memoria> Memorias { get; set; } = new List<Memoria>();
}
