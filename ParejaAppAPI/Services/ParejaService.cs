using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Models.Entities;
using ParejaAppAPI.Models.Responses;
using ParejaAppAPI.Repositories.Interfaces;
using ParejaAppAPI.Services.Interfaces;

namespace ParejaAppAPI.Services;

public class ParejaService : IParejaService
{
    private readonly IParejaRepository _parejaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IEmailService _emailService;

    public ParejaService(
        IParejaRepository parejaRepository, 
        IUsuarioRepository usuarioRepository,
        IEmailService emailService)
    {
        _parejaRepository = parejaRepository;
        _usuarioRepository = usuarioRepository;
        _emailService = emailService;
    }

    public async Task<Response<ParejaResponse?>> GetParejaActivaAsync(int usuarioId)
    {
        try
        {
            var pareja = await _parejaRepository.GetParejaActivaByUsuarioIdAsync(usuarioId);
            
            if (pareja == null)
            {
                // Buscar invitación pendiente (enviada o recibida)
                var invitacionPendiente = await _parejaRepository.Get(p =>
                    (p.UsuarioEnviaId == usuarioId || p.UsuarioRecibeId == usuarioId)
                    && p.Estado == EstadoInvitacion.Pendiente
                    && !p.IsDeleted);

                var pendiente = invitacionPendiente.FirstOrDefault();

                if (pendiente != null)
                {
                    var usuarioEnvia = await _usuarioRepository.GetByIdAsync(pendiente.UsuarioEnviaId);
                    var usuarioRecibe = await _usuarioRepository.GetByIdAsync(pendiente.UsuarioRecibeId);

                    var response = new ParejaResponse(
                        pendiente.Id,
                        pendiente.UsuarioEnviaId,
                        pendiente.UsuarioRecibeId,
                        usuarioEnvia?.Nombre ?? "",
                        usuarioRecibe?.Nombre ?? "",
                        usuarioEnvia?.Email ?? "",
                        usuarioRecibe?.Email ?? "",
                        (int)pendiente.Estado,
                        pendiente.CreatedAt,
                        null,
                        null
                    );
                    return Response<ParejaResponse?>.Success(response, 200);
                }

                return Response<ParejaResponse?>.Success(null, 200);
            }

            var responseData = new ParejaResponse(
                pareja.Id,
                pareja.UsuarioEnviaId,
                pareja.UsuarioRecibeId,
                pareja.UsuarioEnvia?.Nombre ?? "",
                pareja.UsuarioRecibe?.Nombre ?? "",
                pareja.UsuarioEnvia?.Email ?? "",
                pareja.UsuarioRecibe?.Email ?? "",
                (int)pareja.Estado,
                pareja.CreatedAt,
                pareja.UsuarioEnvia?.ProfilePhoto?.UrlPublica,
                pareja.UsuarioRecibe?.ProfilePhoto?.UrlPublica
            );

            return Response<ParejaResponse?>.Success(responseData, 200);
        }
        catch (Exception ex)
        {
            return Response<ParejaResponse?>.Failure(500, "Error al obtener pareja", new[] { ex.Message });
        }
    }

    public async Task<Response<ParejaResponse>> EnviarInvitacionAsync(int usuarioId, EnviarInvitacionDto dto)
    {
        try
        {
            // Verificar que el usuario que envía existe
            var usuarioEnvia = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuarioEnvia == null)
                return Response<ParejaResponse>.Failure(404, "Usuario no encontrado");

            // Buscar usuario receptor por email
            var usuarioRecibe = await _usuarioRepository.GetByEmailAsync(dto.EmailPareja);
            if (usuarioRecibe == null)
                return Response<ParejaResponse>.Failure(404, "No se encontró un usuario con ese email");

            if (usuarioRecibe.Id == usuarioId)
                return Response<ParejaResponse>.Failure(400, "No puedes enviarte una invitación a ti mismo");

            // Verificar si ya tiene pareja activa
            var parejaActiva = await _parejaRepository.GetParejaActivaByUsuarioIdAsync(usuarioId);
            if (parejaActiva != null)
                return Response<ParejaResponse>.Failure(400, "Ya tienes una pareja activa");

            // Verificar si ya existe invitación pendiente
            var invitacionExistente = await _parejaRepository.GetInvitacionPendienteByUsuariosAsync(usuarioId, usuarioRecibe.Id);
            if (invitacionExistente != null)
                return Response<ParejaResponse>.Failure(400, "Ya existe una invitación pendiente con este usuario");

            // Crear nueva invitación
            var pareja = new Pareja
            {
                UsuarioEnviaId = usuarioId,
                UsuarioRecibeId = usuarioRecibe.Id,
                Estado = EstadoInvitacion.Pendiente
            };

            await _parejaRepository.AddAsync(pareja);

            // Enviar email de notificación
            try
            {
                await _emailService.SendInvitacionParejaAsync(
                    usuarioRecibe.Email,
                    usuarioRecibe.Nombre,
                    usuarioEnvia.Nombre,
                    pareja.Id,
                    (int)EstadoInvitacion.Pendiente
                );
            }
            catch (Exception emailEx)
            {
                // Log error pero no fallar la operación
                Console.WriteLine($"Error enviando email: {emailEx.Message}");
            }

            var response = new ParejaResponse(
                pareja.Id,
                pareja.UsuarioEnviaId,
                pareja.UsuarioRecibeId,
                usuarioEnvia.Nombre,
                usuarioRecibe.Nombre,
                usuarioEnvia.Email,
                usuarioRecibe.Email,
                (int)pareja.Estado,
                pareja.CreatedAt
            );

            return Response<ParejaResponse>.Success(response, 201);
        }
        catch (Exception ex)
        {
            return Response<ParejaResponse>.Failure(500, "Error al enviar invitación", new[] { ex.Message });
        }
    }

    public async Task<Response<ParejaResponse>> ResponderInvitacionAsync(int usuarioId, ResponderInvitacionDto dto)
    {
        try
        {
            var pareja = await _parejaRepository.GetByIdAsync(dto.ParejaId);
            if (pareja == null)
                return Response<ParejaResponse>.Failure(404, "Invitación no encontrada");

            // Verificar que el usuario es el receptor de la invitación
            if (pareja.UsuarioRecibeId != usuarioId)
                return Response<ParejaResponse>.Failure(403, "No tienes permiso para responder esta invitación");

            if (pareja.Estado != EstadoInvitacion.Pendiente)
                return Response<ParejaResponse>.Failure(400, "Esta invitación ya fue respondida");

            // Actualizar estado
            pareja.Estado = (EstadoInvitacion)dto.Estado;
            pareja.UpdatedAt = DateTime.UtcNow;

            await _parejaRepository.UpdateAsync(pareja);

            var usuarioEnvia = await _usuarioRepository.GetByIdAsync(pareja.UsuarioEnviaId);
            var usuarioRecibe = await _usuarioRepository.GetByIdAsync(pareja.UsuarioRecibeId);

            var response = new ParejaResponse(
                pareja.Id,
                pareja.UsuarioEnviaId,
                pareja.UsuarioRecibeId,
                usuarioEnvia?.Nombre ?? "",
                usuarioRecibe?.Nombre ?? "",
                usuarioEnvia?.Email ?? "",
                usuarioRecibe?.Email ?? "",
                (int)pareja.Estado,
                pareja.CreatedAt
            );

            return Response<ParejaResponse>.Success(response, 200);
        }
        catch (Exception ex)
        {
            return Response<ParejaResponse>.Failure(500, "Error al responder invitación", new[] { ex.Message });
        }
    }

    public async Task<Response<bool>> EliminarParejaAsync(int usuarioId)
    {
        try
        {
            var pareja = await _parejaRepository.GetParejaActivaByUsuarioIdAsync(usuarioId);
            if (pareja == null)
            {
                // Buscar invitación pendiente (enviada O recibida)
                var invitaciones = await _parejaRepository.GetAllAsync();
                var invitacionPendiente = invitaciones.FirstOrDefault(p => 
                    (p.UsuarioEnviaId == usuarioId || p.UsuarioRecibeId == usuarioId)
                    && p.Estado == EstadoInvitacion.Pendiente 
                    && !p.IsDeleted);

                if (invitacionPendiente == null)
                    return Response<bool>.Failure(404, "No tienes pareja activa o invitación pendiente");

                pareja = invitacionPendiente;
            }

            // Verificar que el usuario es parte de la pareja
            if (pareja.UsuarioEnviaId != usuarioId && pareja.UsuarioRecibeId != usuarioId)
                return Response<bool>.Failure(403, "No tienes permiso para eliminar esta pareja");

            await _parejaRepository.DeleteAsync(pareja.Id);
            return Response<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            return Response<bool>.Failure(500, "Error al eliminar pareja", new[] { ex.Message });
        }
    }
}
