namespace ParejaAppAPI.Models.Entities;

public class Meta : BaseEntity
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Progreso { get; set; } = 0; // 0-100
    public string Estado { get; set; } = "Planificado"; // Planificado, En progreso, Completado
    public int UsuarioId { get; set; }

    // Relaciones
    public virtual Usuario Usuario { get; set; } = null!;
}
