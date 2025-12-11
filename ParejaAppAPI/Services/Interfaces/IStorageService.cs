using ParejaAppAPI.Models.Responses;

namespace ParejaAppAPI.Services.Interfaces;

public interface IStorageService
{
    Task<Response<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder = "memorias");
    Task<Response<bool>> DeleteFileAsync(string fileUrl);
    string GetPublicUrl(string filePath);
}
