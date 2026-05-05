using EduVote.API.Mappers;
using EduVote.API.Services.Tools;
using EduVote.DAL.Postgresql.Repositories;

namespace EduVote.API.Services.Grpc;

public class CandidateService(
    IVotingRepository votingRepository,
    ICandidateRepository candidateRepository)
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
        response.Candidates.AddRange(candidates.MapToResponseList());
        
        return response;
    }

    public override Task<UploadCandidatePhotoResponse> UploadCandidatePhoto(UploadCandidatePhotoRequest request, ServerCallContext context)
    {
        // todo сделать логику загрузки фотограмии для сущности кандидата + хранить ее в s3 и обращаться через свой file storage service
        return base.UploadCandidatePhoto(request, context);
    }
}