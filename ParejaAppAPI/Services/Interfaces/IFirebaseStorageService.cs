using ParejaAppAPI.Models.Entities;
using ParejaAppAPI.Models.Responses;

namespace ParejaAppAPI.Services.Interfaces;

public interface IFirebaseStorageService
{
    Task<Response<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder = "memorias");
    Task<Response<bool>> DeleteFileAsync(string fileUrl);
    string GetPublicUrl(string filePath);
}
