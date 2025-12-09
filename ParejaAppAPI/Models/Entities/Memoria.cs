namespace ParejaAppAPI.Models.Entities;

public class Memoria : BaseEntity
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime FechaMemoria { get; set; }
    public int UsuarioId { get; set; }
    public int? ResourceId { get; set; }

    // Relaciones
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual Resource? Resource { get; set; }
}
