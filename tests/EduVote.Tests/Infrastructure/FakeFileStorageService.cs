using EduVote.Application.Storage;

namespace EduVote.Tests.Infrastructure;

public sealed class FakeFileStorageService : IFileStorageService
{
    public Task<string> UploadFileAsync(
        Stream stream,
        string contentType,
        string objectName,
        Guid objectId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult($"https://storage.test/{objectId}/{objectName}");

    public Task DeleteFileAsync(
        string objectName,
        Guid objectId,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<string> GetPresignedUrlAsync(Guid objectId, string objectName) =>
        Task.FromResult($"https://storage.test/{objectId}/{objectName}");
}
