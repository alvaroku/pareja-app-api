namespace ParejaAppAPI.Models.Entities;

public enum EstadoMeta
{
    Pendiente = 0,
    EnProgreso = 1,
    Completado = 2
}

public class Meta : BaseEntity
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Progreso { get; set; } = 0; // 0-100
    public EstadoMeta Estado { get; set; } = EstadoMeta.Pendiente;
    public int UsuarioId { get; set; }

    // Relaciones
    public virtual Usuario Usuario { get; set; } = null!;
}
