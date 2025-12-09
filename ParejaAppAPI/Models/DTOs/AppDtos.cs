namespace ParejaAppAPI.Models.DTOs;

// Auth DTOs
public record LoginDto(string Email, string Password);
public record RegisterDto(string Nombre, string Email, string Password, string? CodigoPais, string? Telefono);
public record AuthResponse(int Id, string Nombre, string Email, string? CodigoPais, string? Telefono, string Token);

// Usuario DTOs
public record UsuarioResponse(int Id, string Nombre, string Email, string? CodigoPais, string? Telefono);
public record CreateUsuarioDto(string Nombre, string Email, string Password, string? CodigoPais, string? Telefono);
public record UpdateUsuarioDto(string Nombre, string Email, string? CodigoPais, string? Telefono);

// Cita DTOs
public record CitaResponse(int Id, string Titulo, string? Descripcion, DateTime FechaHora, string? Lugar, int UsuarioId);
public record CreateCitaDto(string Titulo, string? Descripcion, DateTime FechaHora, string? Lugar, int UsuarioId);
public record UpdateCitaDto(string Titulo, string? Descripcion, DateTime FechaHora, string? Lugar);

// Meta DTOs
public record MetaResponse(int Id, string Titulo, string? Descripcion, int Progreso, int Estado, int UsuarioId);
public record CreateMetaDto(string Titulo, string? Descripcion, int UsuarioId);
public record UpdateMetaDto(string Titulo, string? Descripcion, int Progreso, int Estado);

// Memoria DTOs
public record MemoriaResponse(int Id, string Titulo, string? Descripcion, string? UrlFoto, DateTime FechaMemoria, int UsuarioId);
public record CreateMemoriaDto(string Titulo, string? Descripcion, string? UrlFoto, DateTime FechaMemoria, int UsuarioId);
public record UpdateMemoriaDto(string Titulo, string? Descripcion, string? UrlFoto, DateTime FechaMemoria);

// Pareja DTOs
public record ParejaResponse(int Id, int UsuarioEnviaId, int UsuarioRecibeId, string UsuarioEnviaNombre, string UsuarioRecibeNombre, string UsuarioEnviaEmail, string UsuarioRecibeEmail, int Estado, DateTime CreatedAt);
public record EnviarInvitacionDto(string EmailPareja);
public record ResponderInvitacionDto(int ParejaId, int Estado); // 1 = Aceptar, 2 = Rechazar
