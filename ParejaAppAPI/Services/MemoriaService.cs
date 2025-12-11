using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Models.Entities;
using ParejaAppAPI.Models.Responses;
using ParejaAppAPI.Repositories.Interfaces;
using ParejaAppAPI.Services.Interfaces;

namespace ParejaAppAPI.Services;

public class MemoriaService : IMemoriaService
{
    private readonly IMemoriaRepository _repository;
    private readonly IParejaRepository _parejaRepository;
    private readonly IResourceRepository _resourceRepository;
    private readonly IStorageService _firebaseStorage;

    public MemoriaService(
        IMemoriaRepository repository,
        IParejaRepository parejaRepository,
        IResourceRepository resourceRepository,
        IStorageService firebaseStorage)
    {
        _repository = repository;
        _parejaRepository = parejaRepository;
        _resourceRepository = resourceRepository;
        _firebaseStorage = firebaseStorage;
    }

    private ResourceResponse? MapResource(Resource? resource)
    {
        if (resource == null) return null;
        return new ResourceResponse(resource.Id, resource.Nombre, resource.Extension, resource.Tamaño, resource.UrlPublica, (int)resource.Tipo);
    }

    public async Task<Response<MemoriaResponse>> GetByIdAsync(int id)
    {
        try
        {
            var memoria = await _repository.GetByIdAsync(id, m => m.Usuario, m => m.Resource);
            if (memoria == null)
                return Response<MemoriaResponse>.Failure(404, "Memoria no encontrada");

            var response = new MemoriaResponse(memoria.Id, memoria.Titulo, memoria.Descripcion, memoria.FechaMemoria, memoria.UsuarioId, MapResource(memoria.Resource));
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
            var response = memorias.Select(m => new MemoriaResponse(m.Id, m.Titulo, m.Descripcion, m.FechaMemoria, m.UsuarioId, MapResource(m.Resource)));
            return Response<IEnumerable<MemoriaResponse>>.Success(response, 200);
        }
        catch (Exception ex)
        {
            return Response<IEnumerable<MemoriaResponse>>.Failure(500, "Error al obtener memorias", new[] { ex.Message });
        }
    }

    public async Task<Response<IEnumerable<MemoriaResponse>>> GetByUsuarioYParejaAsync(int usuarioId)
    {
        try
        {
            var pareja = await _parejaRepository.GetParejaActivaByUsuarioIdAsync(usuarioId);

            if (pareja == null)
            {
                var memoriasUsuario = await _repository.GetByUsuarioIdAsync(usuarioId);
                var responseUsuario = memoriasUsuario.Select(m => new MemoriaResponse(m.Id, m.Titulo, m.Descripcion, m.FechaMemoria, m.UsuarioId, MapResource(m.Resource)));
                return Response<IEnumerable<MemoriaResponse>>.Success(responseUsuario, 200);
            }

            var parejaId = pareja.UsuarioEnviaId == usuarioId ? pareja.UsuarioRecibeId : pareja.UsuarioEnviaId;
            var memorias = await _repository.GetByUsuarioYParejaAsync(usuarioId, parejaId);
            var response = memorias.Select(m => new MemoriaResponse(m.Id, m.Titulo, m.Descripcion, m.FechaMemoria, m.UsuarioId, MapResource(m.Resource)));
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

            var memoriaResponses = items.Select(m => new MemoriaResponse(m.Id, m.Titulo, m.Descripcion, m.FechaMemoria, m.UsuarioId, MapResource(m.Resource)));
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
                FechaMemoria = dto.FechaMemoria,
                UsuarioId = dto.UsuarioId,
                ResourceId = null,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(memoria);
            var response = new MemoriaResponse(memoria.Id, memoria.Titulo, memoria.Descripcion, memoria.FechaMemoria, memoria.UsuarioId, null);
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
            memoria.FechaMemoria = dto.FechaMemoria;
            memoria.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(memoria);

            var memoriaConResource = await _repository.GetByIdAsync(id);
            var response = new MemoriaResponse(memoriaConResource!.Id, memoriaConResource.Titulo, memoriaConResource.Descripcion, memoriaConResource.FechaMemoria, memoriaConResource.UsuarioId, MapResource(memoriaConResource.Resource));
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
            var memoria = await _repository.GetByIdAsync(id, m => m.Resource);
            if (memoria == null)
                return Response<bool>.Failure(404, "Memoria no encontrada");

            // Si tiene recurso asociado, eliminarlo de Firebase y base de datos
            if (memoria.ResourceId.HasValue && memoria.Resource != null)
            {
                // Eliminar de Firebase Storage
                if (!string.IsNullOrEmpty(memoria.Resource.UrlPublica))
                {
                    await _firebaseStorage.DeleteFileAsync(memoria.Resource.UrlPublica);
                }

                // Eliminar registro de Resource
                await _resourceRepository.DeleteAsync(memoria.ResourceId.Value);
            }

            // Eliminar memoria
            await _repository.DeleteAsync(id);
            return Response<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            return Response<bool>.Failure(500, "Error al eliminar memoria", new[] { ex.Message });
        }
    }
}
