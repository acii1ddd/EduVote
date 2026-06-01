using EduVote.API.Mappers;
using EduVote.API.Validators;
using EduVote.Application.Candidates.CreateCandidate;
using EduVote.Application.Candidates.DeleteCandidate;
using EduVote.Application.Candidates.DeleteCandidatePhoto;
using EduVote.Application.Candidates.GetCandidates;
using EduVote.Application.Common;
using MediatR;

namespace EduVote.API.Services;

public class CandidateService(ISender sender) : Candidates.CandidatesBase
{
    public override async Task<CandidateResponse> CreateCandidate(
        AddCandidateRequest request,
        ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.VotingId, "Voting");

        try
        {
            var candidate = await sender.Send(
                new CreateCandidateCommand(votingId, request.Name, request.Description),
                context.CancellationToken);

            return candidate.MapToResponse();
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }
    }

    public override async Task<Empty> DeleteCandidate(
        DeleteCandidateRequest request,
        ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.VotingId, "Voting");
        var candidateId = IdParser.ParseId(request.CandidateId, "Candidate");

        try
        {
            await sender.Send(
                new DeleteCandidateCommand(votingId, candidateId),
                context.CancellationToken);
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }

        return new Empty();
    }

    public override async Task<GetCandidatesResponse> GetCandidates(
        GetCandidatesRequest request,
        ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.VotingId, "Voting");

        try
        {
            var candidates = await sender.Send(
                new GetCandidatesQuery(votingId),
                context.CancellationToken);

            var response = new GetCandidatesResponse();
            response.Candidates.AddRange(candidates.MapToResponseList());
            return response;
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }
    }

    public override async Task<Empty> DeleteCandidatePhoto(
        DeleteCandidatePhotoRequest request,
        ServerCallContext context)
    {
        var candidateId = IdParser.ParseId(request.CandidateId, "Candidate");

        try
        {
            await sender.Send(
                new DeleteCandidatePhotoCommand(candidateId),
                context.CancellationToken);
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }

        return new Empty();
    }

    private static StatusCode MapStatusCode(ApplicationErrorType errorType) =>
        errorType switch
        {
            ApplicationErrorType.InvalidArgument => StatusCode.InvalidArgument,
            ApplicationErrorType.NotFound => StatusCode.NotFound,
            ApplicationErrorType.PermissionDenied => StatusCode.PermissionDenied,
            ApplicationErrorType.FailedPrecondition => StatusCode.FailedPrecondition,
            ApplicationErrorType.AlreadyExists => StatusCode.AlreadyExists,
            ApplicationErrorType.Unauthenticated => StatusCode.Unauthenticated,
            ApplicationErrorType.Unavailable => StatusCode.Unavailable,
            _ => StatusCode.Unknown
        };
}
