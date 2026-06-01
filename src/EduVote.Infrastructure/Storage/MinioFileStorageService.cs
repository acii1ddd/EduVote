using EduVote.Application.Storage;
using Minio;
using Minio.DataModel.Args;
using Microsoft.Extensions.Logging;

namespace EduVote.Infrastructure.Storage;

public sealed class MinioFileStorageService(
    IMinioClient minioClient,
    ILogger<MinioFileStorageService> logger)
    : IFileStorageService
{
    private const string BucketName = "candidates";
    private const int ExpirationSeconds = 604800;

    public async Task<string> UploadFileAsync(
        Stream stream,
        string contentType,
        string objectName,
        Guid objectId,
        CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(cancellationToken);

        var objectPath = GetObjectPath(objectId, objectName);

        var args = new PutObjectArgs()
            .WithBucket(BucketName)
            .WithObject(objectPath)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType);

        await minioClient.PutObjectAsync(args, cancellationToken);

        return await GetPresignedUrlAsync(objectId, objectName);
    }

    public async Task DeleteFileAsync(
        string objectName,
        Guid objectId,
        CancellationToken cancellationToken = default)
    {
        var objectPath = GetObjectPath(objectId, objectName);

        var args = new RemoveObjectArgs()
            .WithBucket(BucketName)
            .WithObject(objectPath);

        await minioClient.RemoveObjectAsync(args, cancellationToken);
    }

    public async Task<string> GetPresignedUrlAsync(Guid objectId, string objectName)
    {
        try
        {
            var fullObjectName = GetObjectPath(objectId, objectName);

            var url = await minioClient.PresignedGetObjectAsync(
                new PresignedGetObjectArgs()
                    .WithBucket(BucketName)
                    .WithObject(fullObjectName)
                    .WithExpiry(ExpirationSeconds));

            logger.LogInformation(
                "Generated presigned URL for object '{ObjectName}': {Url}",
                fullObjectName,
                url);

            return url;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating presigned URL");
            throw;
        }
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        var exists = await minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(BucketName),
            cancellationToken);

        if (exists)
        {
            return;
        }

        await minioClient.MakeBucketAsync(
            new MakeBucketArgs().WithBucket(BucketName),
            cancellationToken);
    }

    private static string GetObjectPath(Guid objectId, string objectName) =>
        $"{objectId}/{objectName}";
}
