using EduVote.Application.Storage;

namespace EduVote.Application.Candidates;

internal static class CandidatePhotoUrlResolver
{
    private const string PicsumPhotoPrefix = "https://picsum.photos/200?random";

    public static async Task<string> ResolveAsync(
        Guid candidateId,
        string? photoObjectName,
        IFileStorageService fileStorageService)
    {
        if (string.IsNullOrEmpty(photoObjectName))
            return string.Empty;

        if (photoObjectName.StartsWith(PicsumPhotoPrefix, StringComparison.Ordinal))
            return photoObjectName;

        return await fileStorageService.GetPresignedUrlAsync(candidateId, photoObjectName);
    }
}
