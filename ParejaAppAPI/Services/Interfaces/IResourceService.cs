using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Models.Responses;

namespace ParejaAppAPI.Services.Interfaces;

public interface IResourceService
{
    Task<Response<ResourceResponse>> UploadForMemoriaAsync(int memoriaId, Stream fileStream, string fileName, string contentType);
    Task<Response<ResourceResponse>> UploadForUsuarioAsync(int usuarioId, Stream fileStream, string fileName, string contentType);
    Task<Response<bool>> DeleteAsync(int resourceId);
    Task<Response<ResourceResponse>> GetByIdAsync(int id);
}
