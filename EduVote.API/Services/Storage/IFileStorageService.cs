namespace EduVote.API.Services.Storage;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream stream, string contentType, 
        string objectName, Guid objectId, CancellationToken cancellationToken = default
    );
    
    Task DeleteFileAsync(string objectName, Guid objectId, CancellationToken cancellationToken = default);
}
