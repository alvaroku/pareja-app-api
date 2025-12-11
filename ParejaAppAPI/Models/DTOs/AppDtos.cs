namespace ParejaAppAPI.Models.DTOs;

// Auth DTOs
public record LoginDto(string Email, string Password);
public record RegisterDto(string Nombre, string Email, string Password, string? CodigoPais, string? Telefono);
public record AuthResponse(int Id, string Nombre, string Email, string? CodigoPais, string? Telefono, ResourceResponse? Resource, string Token, int Role);

// Usuario DTOs
public record UsuarioResponse(int Id, string Nombre, string Email, string? CodigoPais, string? Telefono, ResourceResponse? Resource, int Role);
public record CreateUsuarioDto(string Nombre, string Email, string Password, string? CodigoPais, string? Telefono, int Role);
public record UpdateUsuarioDto(string Nombre, string Email, string? CodigoPais, string? Telefono, int? Role);

// Cita DTOs
public record CitaResponse(int Id, string Titulo, string? Descripcion, DateTime FechaHora, string? Lugar, int UsuarioId);
public record CreateCitaDto(string Titulo, string? Descripcion, DateTime FechaHora, string? Lugar, int UsuarioId);
public record UpdateCitaDto(string Titulo, string? Descripcion, DateTime FechaHora, string? Lugar);

// Meta DTOs
public record MetaResponse(int Id, string Titulo, string? Descripcion, int Progreso, int Estado, int UsuarioId);
public record CreateMetaDto(string Titulo, string? Descripcion, int UsuarioId);
public record UpdateMetaDto(string Titulo, string? Descripcion, int Progreso, int Estado);

// Memoria DTOs
public record MemoriaResponse(int Id, string Titulo, string? Descripcion, DateTime FechaMemoria, int UsuarioId, ResourceResponse? Resource);
public record CreateMemoriaDto(string Titulo, string? Descripcion, DateTime FechaMemoria, int UsuarioId);
public record UpdateMemoriaDto(string Titulo, string? Descripcion, DateTime FechaMemoria);

// Resource DTOs
public record ResourceResponse(int Id, string Nombre, string Extension, long Tamaño, string UrlPublica, int Tipo);
public record UploadResourceDto(string Nombre, string Extension, string ContentType, int Tipo);

// Pareja DTOs
public record ParejaResponse(int Id, int UsuarioEnviaId, int UsuarioRecibeId, string UsuarioEnviaNombre, string UsuarioRecibeNombre, string UsuarioEnviaEmail, string UsuarioRecibeEmail, int Estado, DateTime CreatedAt);
public record EnviarInvitacionDto(string EmailPareja);
public record ResponderInvitacionDto(int ParejaId, int Estado); // 1 = Aceptar, 2 = Rechazar

// Notificacion DTOs
public record NotificationRequest(
    int UserId,
    string Title,
    string Body,
    bool SendImmediately,
    DateTime? ScheduledAtUtc,
    Dictionary<string, string>? AdditionalData
);

public record NotificationResponse(
    int Id,
    int UserId,
    string Title,
    string Body,
    bool IsRead
);

public record SendNotificationRequest(
    string Token,
    string Titulo,
    string Cuerpo,
    Dictionary<string, string>? AdditionalData
);

public record SendSMSRequest(
    string PhoneTo,
    string Message
);


public class DefaultFilterParams : PaginateParams
{
    public string? Search { get; set; }

    public bool? Status { get; set; }
}

public class PaginateParams
{
    public int PageNumber { get; set; } = 1;


    public int PageSize { get; set; } = 10;

}