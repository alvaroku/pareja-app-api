namespace ParejaAppAPI.Models.Entities;

public enum TipoRecurso
{
    Imagen = 0,
    Documento = 1,
    Video = 2,
    Audio = 3
}

public class Resource : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long Tamaño { get; set; } // En bytes
    public string UrlPublica { get; set; } = string.Empty;
    public string? Ubicacion { get; set; } // Ruta en Firebase Storage
    public TipoRecurso Tipo { get; set; }
    public string? ContentType { get; set; }

    // Relaciones
    public virtual Usuario? Usuario { get; set; }
    public virtual Memoria? Memoria { get; set; }
}
