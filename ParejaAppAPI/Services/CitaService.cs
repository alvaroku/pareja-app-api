using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Models.Entities;
using ParejaAppAPI.Models.Responses;
using ParejaAppAPI.Repositories.Interfaces;
using ParejaAppAPI.Services.Interfaces;

namespace ParejaAppAPI.Services;

public class CitaService : ICitaService
{
    private readonly ICitaRepository _repository;

    public CitaService(ICitaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Response<CitaResponse>> GetByIdAsync(int id)
    {
        try
        {
            var cita = await _repository.GetByIdAsync(id, c => c.Usuario);
            if (cita == null)
                return Response<CitaResponse>.Failure(404, "Cita no encontrada");

            var response = new CitaResponse(cita.Id, cita.Titulo, cita.Descripcion, cita.FechaHora, cita.Lugar, cita.UsuarioId);
            return Response<CitaResponse>.Success(response, 200);
        }
        catch (Exception ex)
        {
            return Response<CitaResponse>.Failure(500, "Error al obtener cita", new[] { ex.Message });
        }
    }

    public async Task<Response<IEnumerable<CitaResponse>>> GetByUsuarioIdAsync(int usuarioId)
    {
        try
        {
            var citas = await _repository.GetByUsuarioIdAsync(usuarioId);
            var response = citas.Select(c => new CitaResponse(c.Id, c.Titulo, c.Descripcion, c.FechaHora, c.Lugar, c.UsuarioId));
            return Response<IEnumerable<CitaResponse>>.Success(response, 200);
        }
        catch (Exception ex)
        {
            return Response<IEnumerable<CitaResponse>>.Failure(500, "Error al obtener citas", new[] { ex.Message });
        }
    }

    public async Task<Response<PagedResponse<CitaResponse>>> GetPagedAsync(int pageNumber, int pageSize, int? usuarioId = null)
    {
        try
        {
            var (items, totalCount) = usuarioId.HasValue
                ? await _repository.GetPagedAsync(pageNumber, pageSize, c => c.UsuarioId == usuarioId.Value)
                : await _repository.GetPagedAsync(pageNumber, pageSize);

            var citaResponses = items.Select(c => new CitaResponse(c.Id, c.Titulo, c.Descripcion, c.FechaHora, c.Lugar, c.UsuarioId));
            var pagedResponse = new PagedResponse<CitaResponse>(citaResponses, pageNumber, pageSize, totalCount);
            return Response<PagedResponse<CitaResponse>>.Success(pagedResponse, 200);
        }
        catch (Exception ex)
        {
            return Response<PagedResponse<CitaResponse>>.Failure(500, "Error al obtener citas", new[] { ex.Message });
        }
    }

    public async Task<Response<CitaResponse>> CreateAsync(CreateCitaDto dto)
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
            var response = new CitaResponse(cita.Id, cita.Titulo, cita.Descripcion, cita.FechaHora, cita.Lugar, cita.UsuarioId);
            return Response<CitaResponse>.Success(response, 201);
        }
        catch (Exception ex)
        {
            return Response<CitaResponse>.Failure(500, "Error al crear cita", new[] { ex.Message });
        }
    }

    public async Task<Response<CitaResponse>> UpdateAsync(int id, UpdateCitaDto dto)
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
            var response = new CitaResponse(cita.Id, cita.Titulo, cita.Descripcion, cita.FechaHora, cita.Lugar, cita.UsuarioId);
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
