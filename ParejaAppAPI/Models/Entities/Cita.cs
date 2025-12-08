namespace ParejaAppAPI.Models.Entities;

public class Cita : BaseEntity
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime FechaHora { get; set; }
    public string? Lugar { get; set; }
    public int UsuarioId { get; set; }

    // Relaciones
    public virtual Usuario Usuario { get; set; } = null!;
}
