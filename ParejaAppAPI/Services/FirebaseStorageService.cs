using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using ParejaAppAPI.Models.Responses;
using ParejaAppAPI.Services.Interfaces;

namespace ParejaAppAPI.Services;

public class FirebaseStorageService : IFirebaseStorageService
{
    private readonly string _bucket;
    private readonly string _credentialsPath;

    public FirebaseStorageService(IConfiguration configuration)
    {
        _bucket = configuration["Firebase:StorageBucket"] ?? throw new ArgumentNullException("Firebase:StorageBucket");
        _credentialsPath = configuration["Firebase:CredentialsPath"] ?? throw new ArgumentNullException("Firebase:CredentialsPath");
    }

    private GoogleCredential GetCredentials()
    {
        return GoogleCredential.FromFile(_credentialsPath);
    }

    public async Task<Response<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder = "memorias")
    {
        try
        {
            var credential = GetCredentials();
            var storage = StorageClient.Create(credential);

            // Generar nombre único para el archivo
            var extension = Path.GetExtension(fileName);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{nameWithoutExtension}";
            var filePath = $"{folder}/{uniqueFileName}{extension}";

            // Subir archivo a Firebase Storage
            var result = await storage.UploadObjectAsync(
                _bucket,
                filePath,
                contentType,
                fileStream
            );

            // Construir URL pública
            var downloadUrl = result.MediaLink;

            return Response<string>.Success(downloadUrl, 200);
        }
        catch (Exception ex)
        {
            return Response<string>.Failure(500, "Error al subir archivo", new[] { ex.Message });
        }
    }

    public async Task<Response<bool>> DeleteFileAsync(string fileUrl)
    {
        try
        {
            var credential = GetCredentials();
            var storage = StorageClient.Create(credential);

            // Extraer la ruta del archivo de la URL
            var uri = new Uri(fileUrl);
            var segments = uri.Segments;
            var filePath = Uri.UnescapeDataString(segments[^1].Split('?')[0]);

            // Eliminar archivo de Firebase Storage
            await storage.DeleteObjectAsync(_bucket, filePath);

            return Response<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            return Response<bool>.Failure(500, "Error al eliminar archivo", new[] { ex.Message });
        }
    }

    public string GetPublicUrl(string filePath)
    {
        return $"https://storage.googleapis.com/{_bucket}/{filePath}";
    }
}
