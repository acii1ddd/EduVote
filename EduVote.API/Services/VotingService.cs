using EduVote.API.Mappers;
using EduVote.DAL.Postgresql.Services;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using DataVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;

namespace EduVote.API.Services;

public class VotingService(IVotingRepository votingRepository) 
    : Votings.VotingsBase
{
    public override async Task<VotingResponse> CreateVoting(CreateVotingRequest request, ServerCallContext context)
    {
        ValidateDateRange(request.StartTime, request.EndTime);

        var voting = request.MapToEntity();

        var createdVoting = await votingRepository
            .CreateAsync(voting, context.CancellationToken);
        
        return createdVoting.MapToResponse();
    }

    public override async Task<VotingResponse> UpdateVoting(UpdateVotingRequest request, ServerCallContext context)
    {
        var votingId = ParseVotingId(request.Id);
        ValidateDateRange(request.StartTime, request.EndTime);

        var updatedVoting = request.MapToEntity();

        var updatedVotingResult = await votingRepository
            .UpdateAsync(updatedVoting, context.CancellationToken);
        
        if (updatedVotingResult is null)
        {
            throw ThrowNotFoundException(votingId);
        }

        return updatedVotingResult.MapToResponse();
    }

    public override async Task<Empty> DeleteVoting(DeleteVotingRequest request, ServerCallContext context)
    {
        var votingId = ParseVotingId(request.Id);
        var isDeleted = await votingRepository.DeleteAsync(votingId, context.CancellationToken);

        if (!isDeleted)
        {
            throw ThrowNotFoundException(votingId);
        }

        return new Empty();
    }

    public override async Task<VotingResponse> StartVoting(VotingActionRequest request, ServerCallContext context)
    {
        var votingId = ParseVotingId(request.Id);

        return await ChangeStatus(
            votingId,
            DataVotingStatus.Active,
            [DataVotingStatus.Draft, DataVotingStatus.Paused],
            context.CancellationToken);
    }

    public override async Task<VotingResponse> PauseVoting(VotingActionRequest request, ServerCallContext context)
    {
        var votingId = ParseVotingId(request.Id);

        return await ChangeStatus(
            votingId,
            DataVotingStatus.Paused,
            [DataVotingStatus.Active],
            context.CancellationToken);
    }

    public override async Task<VotingResponse> FinishVoting(VotingActionRequest request, ServerCallContext context)
    {
        var votingId = ParseVotingId(request.Id);

        return await ChangeStatus(
            votingId,
            DataVotingStatus.Finished,
            [DataVotingStatus.Active, DataVotingStatus.Paused],
            context.CancellationToken);
    }

    private async Task<VotingResponse> ChangeStatus(
        Guid votingId,
        DataVotingStatus newStatus,
        IReadOnlyCollection<DataVotingStatus> allowedCurrentStatuses,
        CancellationToken cancellationToken)
    {
        var existingVoting = await votingRepository
            .GetByIdAsync(votingId, cancellationToken);
        
        if (existingVoting is null)
        {
            throw ThrowNotFoundException(votingId);
        }

        if (!allowedCurrentStatuses.Contains(existingVoting.VotingStatus))
        {
            throw new RpcException(new Status(
                StatusCode.FailedPrecondition,
                $"Voting {votingId} cannot be moved from {existingVoting.VotingStatus} to {newStatus}."));
        }

        var updatedVoting = await votingRepository
            .UpdateStatusAsync(votingId, newStatus, cancellationToken);
        
        if (updatedVoting is null)
        {
            throw ThrowNotFoundException(votingId);
        }

        return updatedVoting.MapToResponse();
    }

    private static RpcException ThrowNotFoundException(Guid votingId)
    {
        return new RpcException(new Status(StatusCode.NotFound, $"Voting with id {votingId} was not found."));
    }

    private static Guid ParseVotingId(string id)
    {
        if (!Guid.TryParse(id, out var votingId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Voting id must be a valid GUID."));
        }

        return votingId;
    }

    private static void ValidateDateRange(Timestamp? startTime, Timestamp? endTime)
    {
        if (startTime is null || endTime is null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "start_time and end_time are required."));
        }

        var startDate = startTime.ToDateTime();
        var endDate = endTime.ToDateTime();

        if (Math.Abs((startDate - endDate).TotalSeconds) < 1)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "start_time must be less than end_time."));
        }
    }

    private static DateTime ToUtcDateTime(Timestamp? timestamp)
    {
        return timestamp?.ToDateTime().ToUniversalTime()
               ?? throw new RpcException(new Status(StatusCode.InvalidArgument, "Timestamp value is required."));
    }
}