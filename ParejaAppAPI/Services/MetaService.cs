using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Models.Entities;
using ParejaAppAPI.Models.Responses;
using ParejaAppAPI.Repositories.Interfaces;
using ParejaAppAPI.Services.Interfaces;

namespace ParejaAppAPI.Services;

public class MetaService : IMetaService
{
    private readonly IMetaRepository _repository;
    private readonly IParejaRepository _parejaRepository;

    public MetaService(IMetaRepository repository, IParejaRepository parejaRepository)
    {
        _repository = repository;
        _parejaRepository = parejaRepository;
    }

    public async Task<Response<MetaResponse>> GetByIdAsync(int id)
    {
        try
        {
            var meta = await _repository.GetByIdAsync(id, m => m.Usuario);
            if (meta == null)
                return Response<MetaResponse>.Failure(404, "Meta no encontrada");

            var response = new MetaResponse(meta.Id, meta.Titulo, meta.Descripcion, meta.Progreso, (int)meta.Estado, meta.UsuarioId);
            return Response<MetaResponse>.Success(response, 200);
        }
        catch (Exception ex)
        {
            return Response<MetaResponse>.Failure(500, "Error al obtener meta", new[] { ex.Message });
        }
    }

    public async Task<Response<IEnumerable<MetaResponse>>> GetByUsuarioIdAsync(int usuarioId)
    {
        try
        {
            var metas = await _repository.GetByUsuarioIdAsync(usuarioId);
            var response = metas.Select(m => new MetaResponse(m.Id, m.Titulo, m.Descripcion, m.Progreso, (int)m.Estado, m.UsuarioId));
            return Response<IEnumerable<MetaResponse>>.Success(response, 200);
        }
        catch (Exception ex)
        {
            return Response<IEnumerable<MetaResponse>>.Failure(500, "Error al obtener metas", new[] { ex.Message });
        }
    }

    public async Task<Response<IEnumerable<MetaResponse>>> GetByUsuarioYParejaAsync(int usuarioId)
    {
        try
        {
            var pareja = await _parejaRepository.GetParejaActivaByUsuarioIdAsync(usuarioId);

            if (pareja == null)
            {
                var metasUsuario = await _repository.GetByUsuarioIdAsync(usuarioId);
                var responseUsuario = metasUsuario.Select(m => new MetaResponse(m.Id, m.Titulo, m.Descripcion, m.Progreso, (int)m.Estado, m.UsuarioId));
                return Response<IEnumerable<MetaResponse>>.Success(responseUsuario, 200);
            }

            var parejaId = pareja.UsuarioEnviaId == usuarioId ? pareja.UsuarioRecibeId : pareja.UsuarioEnviaId;
            var metas = await _repository.GetByUsuarioYParejaAsync(usuarioId, parejaId);
            var response = metas.Select(m => new MetaResponse(m.Id, m.Titulo, m.Descripcion, m.Progreso, (int)m.Estado, m.UsuarioId));
            return Response<IEnumerable<MetaResponse>>.Success(response, 200);
        }
        catch (Exception ex)
        {
            return Response<IEnumerable<MetaResponse>>.Failure(500, "Error al obtener metas", new[] { ex.Message });
        }
    }

    public async Task<Response<PagedResponse<MetaResponse>>> GetPagedAsync(int pageNumber, int pageSize, int? usuarioId = null)
    {
        try
        {
            var (items, totalCount) = usuarioId.HasValue
                ? await _repository.GetPagedAsync(pageNumber, pageSize, m => m.UsuarioId == usuarioId.Value)
                : await _repository.GetPagedAsync(pageNumber, pageSize);

            var metaResponses = items.Select(m => new MetaResponse(m.Id, m.Titulo, m.Descripcion, m.Progreso, (int)m.Estado, m.UsuarioId));
            var pagedResponse = new PagedResponse<MetaResponse>(metaResponses, pageNumber, pageSize, totalCount);
            return Response<PagedResponse<MetaResponse>>.Success(pagedResponse, 200);
        }
        catch (Exception ex)
        {
            return Response<PagedResponse<MetaResponse>>.Failure(500, "Error al obtener metas", new[] { ex.Message });
        }
    }

    public async Task<Response<MetaResponse>> CreateAsync(CreateMetaDto dto)
    {
        try
        {
            var meta = new Meta
            {
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                Progreso = 0,
                Estado = EstadoMeta.Pendiente,
                UsuarioId = dto.UsuarioId,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(meta);
            var response = new MetaResponse(meta.Id, meta.Titulo, meta.Descripcion, meta.Progreso, (int)meta.Estado, meta.UsuarioId);
            return Response<MetaResponse>.Success(response, 201);
        }
        catch (Exception ex)
        {
            return Response<MetaResponse>.Failure(500, "Error al crear meta", new[] { ex.Message });
        }
    }

    public async Task<Response<MetaResponse>> UpdateAsync(int id, UpdateMetaDto dto)
    {
        try
        {
            var meta = await _repository.GetByIdAsync(id);
            if (meta == null)
                return Response<MetaResponse>.Failure(404, "Meta no encontrada");

            meta.Titulo = dto.Titulo;
            meta.Descripcion = dto.Descripcion;
            meta.Progreso = dto.Progreso;
            meta.Estado = (EstadoMeta)dto.Estado;
            meta.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(meta);
            var response = new MetaResponse(meta.Id, meta.Titulo, meta.Descripcion, meta.Progreso, (int)meta.Estado, meta.UsuarioId);
            return Response<MetaResponse>.Success(response, 200);
        }
        catch (Exception ex)
        {
            return Response<MetaResponse>.Failure(500, "Error al actualizar meta", new[] { ex.Message });
        }
    }

    public async Task<Response<bool>> DeleteAsync(int id)
    {
        try
        {
            var meta = await _repository.GetByIdAsync(id);
            if (meta == null)
                return Response<bool>.Failure(404, "Meta no encontrada");

            await _repository.DeleteAsync(id);
            return Response<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            return Response<bool>.Failure(500, "Error al eliminar meta", new[] { ex.Message });
        }
    }
}
