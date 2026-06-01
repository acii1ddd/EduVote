using EduVote.API.Mappers;
using EduVote.API.Services.Tools;
using EduVote.Application.Common;
using EduVote.Application.VotingTargets.AddVotingTarget;
using EduVote.Application.VotingTargets.DeleteVotingTarget;
using EduVote.Application.VotingTargets.GetVotingTargets;
using MediatR;

namespace EduVote.API.Services.Grpc;

public class VotingTargetService(ISender sender) : VotingTargets.VotingTargetsBase
{
    public override async Task<VotingTargetResponse> AddTarget(
        AddVotingTargetRequest request,
        ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.VotingId, "Voting");
        var educationUnitId = IdParser.ParseId(request.EducationUnitId, "Education unit");

        try
        {
            var target = await sender.Send(
                new AddVotingTargetCommand(votingId, educationUnitId),
                context.CancellationToken);

            return target.MapToResponse();
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }
    }

    public override async Task<Empty> DeleteTarget(
        DeleteVotingTargetRequest request,
        ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.VotingId, "Voting");
        var educationUnitId = IdParser.ParseId(request.EducationUnitId, "Education unit");

        try
        {
            await sender.Send(
                new DeleteVotingTargetCommand(votingId, educationUnitId),
                context.CancellationToken);
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }

        return new Empty();
    }

    public override async Task<GetVotingTargetsResponse> GetTargets(
        GetVotingTargetsRequest request,
        ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.VotingId, "Voting");

        try
        {
            var targets = await sender.Send(
                new GetVotingTargetsQuery(votingId),
                context.CancellationToken);

            var response = new GetVotingTargetsResponse();
            response.Targets.AddRange(targets.MapToResponseList());

            return response;
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }
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
