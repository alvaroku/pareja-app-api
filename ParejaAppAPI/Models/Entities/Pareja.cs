namespace ParejaAppAPI.Models.Entities;

public enum EstadoInvitacion
{
    Pendiente = 0,
    Aceptada = 1,
    Rechazada = 2
}

public class Pareja : BaseEntity
{
    public int UsuarioEnviaId { get; set; }
    public int UsuarioRecibeId { get; set; }
    public EstadoInvitacion Estado { get; set; } = EstadoInvitacion.Pendiente;

    // Relaciones
    public virtual Usuario? UsuarioEnvia { get; set; }
    public virtual Usuario? UsuarioRecibe { get; set; }
}
