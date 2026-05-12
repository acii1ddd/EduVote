using EduVote.API.Mappers;
using EduVote.API.Services.Storage;
using EduVote.API.Services.Tools;
using EduVote.DAL.Postgresql.Repositories;
using EduVote.DAL.Postgresql.Repositories.Interfaces;

namespace EduVote.API.Services.Grpc;

public class CandidateService(
    IVotingRepository votingRepository,
    ICandidateRepository candidateRepository,
    IFileStorageService fileStorageService,
    ILogger<CandidateService> logger)
    : Candidates.CandidatesBase
{
    public override async Task<CandidateResponse> CreateCandidate(
        AddCandidateRequest request,
        ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.VotingId, "Voting");
        
        var voting = await votingRepository.GetByIdAsync(
            votingId,
            context.CancellationToken
        );

        if (voting is null)
        {
            throw IdParser.CreateNotFoundException("Voting", request.VotingId);
        }

        var candidate = request.MapToEntity();
        
        var createdCandidate = await candidateRepository.CreateAsync(
            candidate,
            context.CancellationToken
        );

        return createdCandidate.MapToResponse();
    }

    public override async Task<Empty> DeleteCandidate(
        DeleteCandidateRequest request,
        ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.VotingId, "Voting");
        
        var voting = await votingRepository.GetByIdAsync(
            votingId,
            context.CancellationToken
        );

        if (voting is null)
        {
            throw IdParser.CreateNotFoundException("Voting", request.VotingId);
        }

        var candidateId = IdParser.ParseId(request.CandidateId, "Candidate");
        
        var candidate = await candidateRepository.GetByIdAsync(
            candidateId,
            context.CancellationToken
        );

        if (candidate is null)
        {
            throw IdParser.CreateNotFoundException("Candidate", request.CandidateId);
        }

        await candidateRepository
            .DeleteAsync(candidate, context.CancellationToken);
        
        return new Empty();
    }

    public override async Task<GetCandidatesResponse> GetCandidates(
        GetCandidatesRequest request,
        ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.VotingId, "Voting");
        
        var voting = await votingRepository.GetByIdAsync(
            votingId,
            context.CancellationToken
        );

        if (voting is null)
        {
            throw IdParser.CreateNotFoundException("Voting", request.VotingId);
        }
        
        var candidates = await candidateRepository.GetByVotingIdAsync(
            votingId,
            context.CancellationToken
        );

        var response = new GetCandidatesResponse();
        
        // photos with object names
        var candidateResponses = candidates
            .MapToResponseList()
            .ToList();

        // replace photo object names with PresignedURLs
        foreach (var candidate in candidateResponses)
        {
            // random photos from picsum don't have object names and don't need presigned URLs
            if(candidate.PhotoUrl.StartsWith("https://picsum.photos/200?random")) continue;

            // skip candidates without photos
            if(candidate.PhotoUrl == "") continue;
            
            candidate.PhotoUrl = await fileStorageService
                .GetPresignedUrlAsync(Guid.Parse(candidate.Id), candidate.PhotoUrl);
        }
        
        response.Candidates.AddRange(candidateResponses);
        
        return response;
    }

    public override async Task<Empty> DeleteCandidatePhoto(
        DeleteCandidatePhotoRequest request,
        ServerCallContext context)
    {
        var candidateId = IdParser.ParseId(request.CandidateId, "Candidate");

        var candidate = await candidateRepository.GetByIdAsync(
            candidateId, context.CancellationToken);

        if (candidate is null)
            throw IdParser.CreateNotFoundException("Candidate", request.CandidateId);

        if (candidate.PhotoObjectName is null)
            return new Empty();
        
        await fileStorageService
            .DeleteFileAsync(candidate.PhotoObjectName, candidate.Id, context.CancellationToken);

        candidate.PhotoObjectName = "";
        await candidateRepository.SaveChangesAsync(context.CancellationToken);

        return new Empty();
    }
}