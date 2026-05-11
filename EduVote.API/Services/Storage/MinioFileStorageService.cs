using Minio;
using Minio.DataModel.Args;

namespace EduVote.API.Services.Storage;

public class MinioFileStorageService(
    IMinioClient minioClient,
    ILogger<MinioFileStorageService> logger) 
    : IFileStorageService
{
    private const string BucketName = "candidates";
    private const int ExpirationSeconds = 604800; // 7 days
    
    public async Task<string> UploadFileAsync(
        Stream stream,
        string contentType,
        string objectName,
        Guid objectId,
        CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(cancellationToken);

        // Create object name with objectId as prefix to avoid name collisions
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

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        var exists = await minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(BucketName),
            cancellationToken);

        if (exists) return;

        await minioClient.MakeBucketAsync(
            new MakeBucketArgs().WithBucket(BucketName),
            cancellationToken);
    }

    private async Task<string> GetPresignedUrlAsync(
        Guid candidateId,
        string fileName)
    {
        try
        {
            var objectName = GetObjectPath(candidateId, fileName);

            var url = await minioClient.PresignedGetObjectAsync(
                new PresignedGetObjectArgs()
                    .WithBucket(BucketName)
                    .WithObject(objectName)
                    .WithExpiry(ExpirationSeconds));

            logger.LogInformation("Generated presigned URL for object '{ObjectName}': {Url}", objectName, url);
            
            return url;
        }
        catch (Exception ex)
        {
            logger.LogError("Error generating presigned URL: {ExMessage}", ex.Message);
            throw;
        }
    }
    
    private static string GetObjectPath(Guid objectId, string objectName) =>
        $"{objectId}/{objectName}";
}
