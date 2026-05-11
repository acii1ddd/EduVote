using Microsoft.Extensions.Logging;
using Minio;
using Minio.ApiEndpoints;
using Minio.DataModel;
using Minio.DataModel.Args;
using Thesis.Application.Interfaces;

namespace Thesis.Infrastructure.Services;

public class MinioImageService : IImageService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName = "project-images";
    private readonly ILogger<MinioImageService> _logger;

    public MinioImageService(IMinioClient minioClient, ILogger<MinioImageService> logger)
    {
        _minioClient = minioClient ?? throw new ArgumentNullException(nameof(minioClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogInformation("MinioImageService initialized with client: {ClientType}", _minioClient.GetType().Name);
    }

    public async Task<string> UploadImageAsync(
        Guid projectId,
        string fileName,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken)
    {
        try
        {
            // Ensure bucket exists
            await EnsureBucketExistsAsync(cancellationToken);

            // Create object name with project structure
            var objectName = GetObjectPath(projectId, fileName);

            // Upload the file
            await _minioClient.PutObjectAsync(
                new PutObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName)
                    .WithStreamData(fileStream)
                    .WithObjectSize(fileStream.Length)
                    .WithContentType(contentType),
                cancellationToken);

            _logger.LogInformation($"Image uploaded successfully: {objectName}");

            // Generate and return the URL
            return await GetPresignedUrlAsync(projectId, fileName, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error uploading image: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteImageAsync(
        Guid projectId,
        string fileName,
        CancellationToken cancellationToken)
    {
        try
        {
            // Ensure bucket exists
            await EnsureBucketExistsAsync(cancellationToken);

            var objectName = GetObjectPath(projectId, fileName);

            await _minioClient.RemoveObjectAsync(
                new RemoveObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName),
                cancellationToken);

            _logger.LogInformation($"Image deleted successfully: {objectName}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting image: {ex.Message}");
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetProjectImagesAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        try
        {
            await EnsureBucketExistsAsync(cancellationToken);

            var projectPrefix = $"{projectId}/";

            var imageUrls = new List<string>();

            var listArgs = new ListObjectsArgs()
                .WithBucket(_bucketName)
                .WithPrefix(projectPrefix)
                .WithRecursive(true);

            var observable = _minioClient.ListObjectsAsync(
                listArgs,
                cancellationToken
            );

            var items = new List<Item>();

            var completion = new TaskCompletionSource<bool>();

            observable.Subscribe(
                item =>
                {
                    items.Add(item);
                },
                ex =>
                {
                    completion.SetException(ex);
                },
                () =>
                {
                    completion.SetResult(true);
                });

            await completion.Task;

            foreach (var obj in items)
            {
                if (!obj.IsDir)
                {
                    var fileName = Path.GetFileName(obj.Key);

                    var url = await GetPresignedUrlAsync(
                        projectId,
                        fileName
                    );

                    imageUrls.Add(url);
                }
            }

            return imageUrls;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting project images"
            );

            return Enumerable.Empty<string>();
        }
    }

    public async Task<string> GetPresignedUrlAsync(
        Guid projectId,
        string fileName,
        int expirationSeconds = 604800,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var objectName = GetObjectPath(projectId, fileName);

            var url = await _minioClient.PresignedGetObjectAsync(
                new PresignedGetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName)
                    .WithExpiry(expirationSeconds));

            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating presigned URL: {ex.Message}");
            throw;
        }
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var bucketExists = await _minioClient.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(_bucketName),
                cancellationToken);

            if (!bucketExists)
            {
                await _minioClient.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(_bucketName),
                    cancellationToken);

                _logger.LogInformation($"Created bucket: {_bucketName}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error ensuring bucket exists: {ex.Message}");
            throw;
        }
    }

    private static string GetObjectPath(Guid projectId, string fileName) =>
        $"project-images/{projectId}/{fileName}";
}
