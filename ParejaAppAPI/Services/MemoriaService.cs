using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Models.Entities;
using ParejaAppAPI.Models.Responses;
using ParejaAppAPI.Repositories.Interfaces;
using ParejaAppAPI.Services.Interfaces;

namespace ParejaAppAPI.Services;

public class MemoriaService : IMemoriaService
{
    private readonly IMemoriaRepository _repository;

    public MemoriaService(IMemoriaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Response<MemoriaResponse>> GetByIdAsync(int id)
    {
        try
        {
            var memoria = await _repository.GetByIdAsync(id, m => m.Usuario);
            if (memoria == null)
                return Response<MemoriaResponse>.Failure(404, "Memoria no encontrada");

            var response = new MemoriaResponse(memoria.Id, memoria.Titulo, memoria.Descripcion, memoria.UrlFoto, memoria.FechaMemoria, memoria.UsuarioId);
            return Response<MemoriaResponse>.Success(response, 200);
        }
        catch (Exception ex)
        {
            return Response<MemoriaResponse>.Failure(500, "Error al obtener memoria", new[] { ex.Message });
        }
    }

    public async Task<Response<IEnumerable<MemoriaResponse>>> GetByUsuarioIdAsync(int usuarioId)
    {
        try
        {
            var memorias = await _repository.GetByUsuarioIdAsync(usuarioId);
            var response = memorias.Select(m => new MemoriaResponse(m.Id, m.Titulo, m.Descripcion, m.UrlFoto, m.FechaMemoria, m.UsuarioId));
            return Response<IEnumerable<MemoriaResponse>>.Success(response, 200);
        }
        catch (Exception ex)
        {
            return Response<IEnumerable<MemoriaResponse>>.Failure(500, "Error al obtener memorias", new[] { ex.Message });
        }
    }

    public async Task<Response<PagedResponse<MemoriaResponse>>> GetPagedAsync(int pageNumber, int pageSize, int? usuarioId = null)
    {
        try
        {
            var (items, totalCount) = usuarioId.HasValue
                ? await _repository.GetPagedAsync(pageNumber, pageSize, m => m.UsuarioId == usuarioId.Value)
                : await _repository.GetPagedAsync(pageNumber, pageSize);

            var memoriaResponses = items.Select(m => new MemoriaResponse(m.Id, m.Titulo, m.Descripcion, m.UrlFoto, m.FechaMemoria, m.UsuarioId));
            var pagedResponse = new PagedResponse<MemoriaResponse>(memoriaResponses, pageNumber, pageSize, totalCount);
            return Response<PagedResponse<MemoriaResponse>>.Success(pagedResponse, 200);
        }
        catch (Exception ex)
        {
            return Response<PagedResponse<MemoriaResponse>>.Failure(500, "Error al obtener memorias", new[] { ex.Message });
        }
    }

    public async Task<Response<MemoriaResponse>> CreateAsync(CreateMemoriaDto dto)
    {
        try
        {
            var memoria = new Memoria
            {
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                UrlFoto = dto.UrlFoto,
                FechaMemoria = dto.FechaMemoria,
                UsuarioId = dto.UsuarioId,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(memoria);
            var response = new MemoriaResponse(memoria.Id, memoria.Titulo, memoria.Descripcion, memoria.UrlFoto, memoria.FechaMemoria, memoria.UsuarioId);
            return Response<MemoriaResponse>.Success(response, 201);
        }
        catch (Exception ex)
        {
            return Response<MemoriaResponse>.Failure(500, "Error al crear memoria", new[] { ex.Message });
        }
    }

    public async Task<Response<MemoriaResponse>> UpdateAsync(int id, UpdateMemoriaDto dto)
    {
        try
        {
            var memoria = await _repository.GetByIdAsync(id);
            if (memoria == null)
                return Response<MemoriaResponse>.Failure(404, "Memoria no encontrada");

            memoria.Titulo = dto.Titulo;
            memoria.Descripcion = dto.Descripcion;
            memoria.UrlFoto = dto.UrlFoto;
            memoria.FechaMemoria = dto.FechaMemoria;
            memoria.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(memoria);
            var response = new MemoriaResponse(memoria.Id, memoria.Titulo, memoria.Descripcion, memoria.UrlFoto, memoria.FechaMemoria, memoria.UsuarioId);
            return Response<MemoriaResponse>.Success(response, 200);
        }
        catch (Exception ex)
        {
            return Response<MemoriaResponse>.Failure(500, "Error al actualizar memoria", new[] { ex.Message });
        }
    }

    public async Task<Response<bool>> DeleteAsync(int id)
    {
        try
        {
            var memoria = await _repository.GetByIdAsync(id);
            if (memoria == null)
                return Response<bool>.Failure(404, "Memoria no encontrada");

            await _repository.DeleteAsync(id);
            return Response<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            return Response<bool>.Failure(500, "Error al eliminar memoria", new[] { ex.Message });
        }
    }
}
