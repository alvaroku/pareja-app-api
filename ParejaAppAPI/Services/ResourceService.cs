using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Models.Entities;
using ParejaAppAPI.Models.Responses;
using ParejaAppAPI.Repositories.Interfaces;
using ParejaAppAPI.Services.Interfaces;

namespace ParejaAppAPI.Services;

public class ResourceService : IResourceService
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IMemoriaRepository _memoriaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IStorageService _firebaseStorage;

    public ResourceService(
        IResourceRepository resourceRepository, 
        IMemoriaRepository memoriaRepository, 
        IUsuarioRepository usuarioRepository,
        IStorageService firebaseStorage)
    {
        _resourceRepository = resourceRepository;
        _memoriaRepository = memoriaRepository;
        _usuarioRepository = usuarioRepository;
        _firebaseStorage = firebaseStorage;
    }

    public async Task<Response<ResourceResponse>> UploadForMemoriaAsync(int memoriaId, Stream fileStream, string fileName, string contentType)
    {
        try
        {
            var memoria = await _memoriaRepository.GetByIdAsync(memoriaId);
            if (memoria == null)
                return Response<ResourceResponse>.Failure(404, "Memoria no encontrada");

            // Si ya tiene un Resource, eliminar el anterior de Firebase
            if (memoria.ResourceId.HasValue)
            {
                var oldResource = await _resourceRepository.GetByIdAsync(memoria.ResourceId.Value);
                if (oldResource != null && !string.IsNullOrEmpty(oldResource.UrlPublica))
                {
                    await _firebaseStorage.DeleteFileAsync(oldResource.UrlPublica);
                    await _resourceRepository.DeleteAsync(oldResource.Id);
                }
            }

            // Subir nuevo archivo a Firebase
            var uploadResult = await _firebaseStorage.UploadFileAsync(fileStream, fileName, contentType, "memorias");
            if (!uploadResult.IsSuccess)
                return Response<ResourceResponse>.Failure(uploadResult.StatusCode, uploadResult.Message, uploadResult?.Errors?.ToArray());

            // Determinar tipo de recurso por extensión
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var tipo = DeterminarTipoRecurso(extension);

            // Crear registro de Resource
            var resource = new Resource
            {
                Nombre = fileName,
                Extension = extension,
                Tamaño = fileStream.Length,
                UrlPublica = uploadResult.Data!,
                Ubicacion = $"memorias/{fileName}",
                Tipo = tipo,
                ContentType = contentType,
                CreatedAt = DateTime.UtcNow
            };

            await _resourceRepository.AddAsync(resource);

            // Actualizar Memoria con ResourceId
            memoria.Resource = resource;
            memoria.UpdatedAt = DateTime.UtcNow;
            await _memoriaRepository.UpdateAsync(memoria);

            var response = new ResourceResponse(resource.Id, resource.Nombre, resource.Extension, resource.Tamaño, resource.UrlPublica, (int)resource.Tipo);
            return Response<ResourceResponse>.Success(response, 201);
        }
        catch (Exception ex)
        {
            return Response<ResourceResponse>.Failure(500, "Error al subir archivo", new[] { ex.Message });
        }
    }

    public async Task<Response<bool>> DeleteAsync(int resourceId)
    {
        try
        {
            var resource = await _resourceRepository.GetByIdAsync(resourceId);
            if (resource == null)
                return Response<bool>.Failure(404, "Resource no encontrado");

            // Eliminar de Firebase
            if (!string.IsNullOrEmpty(resource.UrlPublica))
            {
                var deleteResult = await _firebaseStorage.DeleteFileAsync(resource.UrlPublica);
                if (!deleteResult.IsSuccess)
                    return Response<bool>.Failure(deleteResult.StatusCode, deleteResult.Message, deleteResult.Errors.ToArray());
            }

            // Eliminar de base de datos
            await _resourceRepository.DeleteAsync(resourceId);
            return Response<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            return Response<bool>.Failure(500, "Error al eliminar resource", new[] { ex.Message });
        }
    }

    public async Task<Response<ResourceResponse>> GetByIdAsync(int id)
    {
        try
        {
            var resource = await _resourceRepository.GetByIdAsync(id);
            if (resource == null)
                return Response<ResourceResponse>.Failure(404, "Resource no encontrado");

            var response = new ResourceResponse(resource.Id, resource.Nombre, resource.Extension, resource.Tamaño, resource.UrlPublica, (int)resource.Tipo);
            return Response<ResourceResponse>.Success(response, 200);
        }
        catch (Exception ex)
        {
            return Response<ResourceResponse>.Failure(500, "Error al obtener resource", new[] { ex.Message });
        }
    }

    public async Task<Response<ResourceResponse>> UploadForUsuarioAsync(int usuarioId, Stream fileStream, string fileName, string contentType)
    {
        try
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario == null)
                return Response<ResourceResponse>.Failure(404, "Usuario no encontrado");

            // Si ya tiene un Resource, eliminar el anterior de Firebase
            if (usuario.ProfilePhotoId.HasValue)
            {
                var oldResource = await _resourceRepository.GetByIdAsync(usuario.ProfilePhotoId.Value);
                if (oldResource != null && !string.IsNullOrEmpty(oldResource.UrlPublica))
                {
                    await _firebaseStorage.DeleteFileAsync(oldResource.UrlPublica);
                    await _resourceRepository.DeleteAsync(oldResource.Id);
                }
            }

            // Subir nuevo archivo a Firebase
            var uploadResult = await _firebaseStorage.UploadFileAsync(fileStream, fileName, contentType, "usuarios");
            if (!uploadResult.IsSuccess)
                return Response<ResourceResponse>.Failure(uploadResult.StatusCode, uploadResult.Message, uploadResult?.Errors?.ToArray());

            // Determinar tipo de recurso por extensión
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var tipo = DeterminarTipoRecurso(extension);

            // Crear registro de Resource
            var resource = new Resource
            {
                Nombre = fileName,
                Extension = extension,
                Tamaño = fileStream.Length,
                UrlPublica = uploadResult.Data!,
                Ubicacion = $"usuarios/{fileName}",
                Tipo = tipo,
                ContentType = contentType,
                CreatedAt = DateTime.UtcNow
            };

            await _resourceRepository.AddAsync(resource);

            // Actualizar Usuario con ResourceId
            usuario.ProfilePhoto = resource;
            usuario.UpdatedAt = DateTime.UtcNow;
            await _usuarioRepository.UpdateAsync(usuario);

            var response = new ResourceResponse(resource.Id, resource.Nombre, resource.Extension, resource.Tamaño, resource.UrlPublica, (int)resource.Tipo);
            return Response<ResourceResponse>.Success(response, 201);
        }
        catch (Exception ex)
        {
            return Response<ResourceResponse>.Failure(500, "Error al subir foto de perfil", new[] { ex.Message });
        }
    }

    private static TipoRecurso DeterminarTipoRecurso(string extension)
    {
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp" => TipoRecurso.Imagen,
            ".mp4" or ".avi" or ".mov" or ".wmv" or ".flv" => TipoRecurso.Video,
            ".mp3" or ".wav" or ".flac" or ".aac" => TipoRecurso.Audio,
            _ => TipoRecurso.Documento
        };
    }
}
