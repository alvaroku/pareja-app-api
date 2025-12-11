using Microsoft.AspNetCore.Identity;
using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Models.Entities;
using ParejaAppAPI.Models.Responses;
using ParejaAppAPI.Repositories.Interfaces;
using ParejaAppAPI.Services.Interfaces;
using ParejaAppAPI.Utils;

namespace ParejaAppAPI.Services;

public class CitaService : ICitaService
{
    private readonly ICitaRepository _repository;
    private readonly IParejaRepository _parejaRepository;
    private readonly IUsuarioService _usuarioService;

    public CitaService(ICitaRepository repository, IParejaRepository parejaRepository, IUsuarioService usuarioService)
    {
        _repository = repository;
        _parejaRepository = parejaRepository;
        _usuarioService = usuarioService;
    }

    public async Task<Response<CitaResponse>> GetByIdAsync(int id, int currentUserId)
    {
        try
        {
            var cita = await _repository.GetByIdAsync(id, c => c.Usuario);
            if (cita == null)
                return Response<CitaResponse>.Failure(404, "Cita no encontrada");
           
            var fechaHoraLocal = cita.FechaHora.ToTimeZone(await _usuarioService.GetUserTimeZoneAsync(currentUserId));
            var response = new CitaResponse(cita.Id, cita.Titulo, cita.Descripcion, fechaHoraLocal, cita.Lugar, cita.UsuarioId);
            return Response<CitaResponse>.Success(response, 200);
        }
        catch (Exception ex)
        {
            return Response<CitaResponse>.Failure(500, "Error al obtener cita", new[] { ex.Message });
        }
    }

    public async Task<Response<IEnumerable<CitaResponse>>> GetByUsuarioIdAsync(int usuarioId, int currentUserId)
    {
        try
        {
            var citas = await _repository.GetByUsuarioIdAsync(usuarioId);
            string? timeZone = await _usuarioService.GetUserTimeZoneAsync(currentUserId);
            var response = citas.Select(c => new CitaResponse(c.Id, c.Titulo, c.Descripcion, c.FechaHora.ToTimeZone(timeZone), c.Lugar, c.UsuarioId));
            return Response<IEnumerable<CitaResponse>>.Success(response, 200);
        }
        catch (Exception ex)
        {
            return Response<IEnumerable<CitaResponse>>.Failure(500, "Error al obtener citas", new[] { ex.Message });
        }
    }

    public async Task<Response<IEnumerable<CitaResponse>>> GetByUsuarioYParejaAsync(int currentUserId)
    {
        try
        {
            // Obtener pareja activa del usuario
            var pareja = await _parejaRepository.GetParejaActivaByUsuarioIdAsync(currentUserId);
            string? timeZone = await _usuarioService.GetUserTimeZoneAsync(currentUserId);
            if (pareja == null)
            {
                // Si no tiene pareja, devolver solo sus citas
                var citasUsuario = await _repository.GetByUsuarioIdAsync(currentUserId);
                var responseUsuario = citasUsuario.Select(c => new CitaResponse(c.Id, c.Titulo, c.Descripcion, c.FechaHora.ToTimeZone(timeZone), c.Lugar, c.UsuarioId));
                return Response<IEnumerable<CitaResponse>>.Success(responseUsuario, 200);
            }

            // Determinar el ID de la pareja
            var parejaId = pareja.UsuarioEnviaId == currentUserId ? pareja.UsuarioRecibeId : pareja.UsuarioEnviaId;

            // Obtener citas del usuario y su pareja
            var citas = await _repository.GetByUsuarioYParejaAsync(currentUserId, parejaId);
            var response = citas.Select(c => new CitaResponse(c.Id, c.Titulo, c.Descripcion, c.FechaHora.ToTimeZone(timeZone), c.Lugar, c.UsuarioId));
            return Response<IEnumerable<CitaResponse>>.Success(response, 200);
        }
        catch (Exception ex)
        {
            return Response<IEnumerable<CitaResponse>>.Failure(500, "Error al obtener citas", new[] { ex.Message });
        }
    }

    public async Task<Response<PagedResponse<CitaResponse>>> GetPagedAsync(int pageNumber, int pageSize, int currentUserId, int? usuarioId = null)
    {
        try
        {
            var (items, totalCount) = usuarioId.HasValue
                ? await _repository.GetPagedAsync(pageNumber, pageSize, c => c.UsuarioId == usuarioId.Value)
                : await _repository.GetPagedAsync(pageNumber, pageSize);

            string? timeZone = await _usuarioService.GetUserTimeZoneAsync(currentUserId);
            var citaResponses = items.Select(c => new CitaResponse(c.Id, c.Titulo, c.Descripcion, c.FechaHora.ToTimeZone(timeZone), c.Lugar, c.UsuarioId));
            var pagedResponse = new PagedResponse<CitaResponse>(citaResponses, pageNumber, pageSize, totalCount);
            return Response<PagedResponse<CitaResponse>>.Success(pagedResponse, 200);
        }
        catch (Exception ex)
        {
            return Response<PagedResponse<CitaResponse>>.Failure(500, "Error al obtener citas", new[] { ex.Message });
        }
    }

    public async Task<Response<CitaResponse>> CreateAsync(CreateCitaDto dto, int currentUserId)
    {
        try
        {
            var cita = new Cita
            {
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                FechaHora = dto.FechaHora,
                Lugar = dto.Lugar,
                UsuarioId = dto.UsuarioId,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(cita);

            string? timeZone = await _usuarioService.GetUserTimeZoneAsync(currentUserId);
            // Devolver la fecha en el timezone del usuario
            var fechaHoraLocal = cita.FechaHora.ToTimeZone(timeZone);
            var response = new CitaResponse(cita.Id, cita.Titulo, cita.Descripcion, fechaHoraLocal, cita.Lugar, cita.UsuarioId);
            return Response<CitaResponse>.Success(response, 201);
        }
        catch (Exception ex)
        {
            return Response<CitaResponse>.Failure(500, "Error al crear cita", new[] { ex.Message });
        }
    }

    public async Task<Response<CitaResponse>> UpdateAsync(int id, UpdateCitaDto dto, int currentUserId)
    {
        try
        {
            var cita = await _repository.GetByIdAsync(id);
            if (cita == null)
                return Response<CitaResponse>.Failure(404, "Cita no encontrada");
            
            cita.Titulo = dto.Titulo;
            cita.Descripcion = dto.Descripcion;
            cita.FechaHora = dto.FechaHora;
            cita.Lugar = dto.Lugar;
            cita.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(cita);

            string? timeZone = await _usuarioService.GetUserTimeZoneAsync(currentUserId);
            // Devolver la fecha en el timezone del usuario
            var fechaHoraLocal = cita.FechaHora.ToTimeZone(timeZone);
            var response = new CitaResponse(cita.Id, cita.Titulo, cita.Descripcion, fechaHoraLocal, cita.Lugar, cita.UsuarioId);
            return Response<CitaResponse>.Success(response, 200);
        }
        catch (Exception ex)
        {
            return Response<CitaResponse>.Failure(500, "Error al actualizar cita", new[] { ex.Message });
        }
    }

    public async Task<Response<bool>> DeleteAsync(int id)
    {
        try
        {
            var cita = await _repository.GetByIdAsync(id);
            if (cita == null)
                return Response<bool>.Failure(404, "Cita no encontrada");

            await _repository.DeleteAsync(id);
            return Response<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            return Response<bool>.Failure(500, "Error al eliminar cita", new[] { ex.Message });
        }
    }
}
