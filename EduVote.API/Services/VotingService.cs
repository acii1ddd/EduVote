using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Services;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using DataVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DataVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.API.Services;

public class VotingService(IVotingRepository votingRepository) : Votings.VotingsBase
{
    public override async Task<VotingResponse> CreateVoting(CreateVotingRequest request, ServerCallContext context)
    {
        ValidateDateRange(request.StartTime, request.EndTime);

        var voting = new DAL.Postgresql.Models.Voting()
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Type = MapVotingType(request.Type),
            IsAnonymous = request.IsAnonymous,
            AllowVoteChange = request.AllowVoteChange,
            StartTime = ToUtcDateTime(request.StartTime),
            EndTime = ToUtcDateTime(request.EndTime),
            VotingStatus = DataVotingStatus.Draft
        };

        var createdVoting = await votingRepository.CreateAsync(voting, context.CancellationToken);
        return MapVoting(createdVoting);
    }

    public override async Task<VotingResponse> UpdateVoting(UpdateVotingRequest request, ServerCallContext context)
    {
        var votingId = ParseVotingId(request.Id);
        ValidateDateRange(request.StartTime, request.EndTime);

        var updatedVotingData = new DAL.Postgresql.Models.Voting
        {
            Id = votingId,
            Title = request.Title,
            Description = request.Description,
            Type = MapVotingType(request.Type),
            IsAnonymous = request.IsAnonymous,
            AllowVoteChange = request.AllowVoteChange,
            StartTime = ToUtcDateTime(request.StartTime),
            EndTime = ToUtcDateTime(request.EndTime)
        };

        var updatedVoting = await votingRepository.UpdateAsync(updatedVotingData, context.CancellationToken);
        if (updatedVoting is null)
        {
            throw CreateNotFoundException(votingId);
        }

        return MapVoting(updatedVoting);
    }

    public override async Task<Empty> DeleteVoting(DeleteVotingRequest request, ServerCallContext context)
    {
        var votingId = ParseVotingId(request.Id);
        var isDeleted = await votingRepository.DeleteAsync(votingId, context.CancellationToken);

        if (!isDeleted)
        {
            throw CreateNotFoundException(votingId);
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
        var existingVoting = await votingRepository.GetByIdAsync(votingId, cancellationToken);
        if (existingVoting is null)
        {
            throw CreateNotFoundException(votingId);
        }

        if (!allowedCurrentStatuses.Contains(existingVoting.VotingStatus))
        {
            throw new RpcException(new Status(
                StatusCode.FailedPrecondition,
                $"Voting {votingId} cannot be moved from {existingVoting.VotingStatus} to {newStatus}."));
        }

        var updatedVoting = await votingRepository.UpdateStatusAsync(votingId, newStatus, cancellationToken);
        if (updatedVoting is null)
        {
            throw CreateNotFoundException(votingId);
        }

        return MapVoting(updatedVoting);
    }

    private static RpcException CreateNotFoundException(Guid votingId)
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

        if (startDate > endDate)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "start_time must be less than or equal to end_time."));
        }
    }

    private static DateTime ToUtcDateTime(Timestamp? timestamp)
    {
        return timestamp?.ToDateTime().ToUniversalTime()
               ?? throw new RpcException(new Status(StatusCode.InvalidArgument, "Timestamp value is required."));
    }

    private static DataVotingType MapVotingType(VotingType votingType)
    {
        return votingType switch
        {
            VotingType.SingleChoice => DataVotingType.SingleChoice,
            VotingType.MultipleChoice => DataVotingType.MultipleChoice,
            VotingType.Rating => DataVotingType.Rating,
            VotingType.OpenAnswer => DataVotingType.OpenAnswer,
            _ => throw new RpcException(new Status(StatusCode.InvalidArgument, "Voting type is not valid."))
        };
    }
    
    private static VotingResponse MapVoting(DAL.Postgresql.Models.Voting source)
    {
        return new VotingResponse
        {
            Id = source.Id.ToString(),
            Title = source.Title,
            Description = source.Description,
            Type = source.Type switch
            {
                DataVotingType.SingleChoice => VotingType.SingleChoice,
                DataVotingType.MultipleChoice => VotingType.MultipleChoice,
                DataVotingType.Rating => VotingType.Rating,
                DataVotingType.OpenAnswer => VotingType.OpenAnswer,
                _ => (VotingType)0
            },
            IsAnonymous = source.IsAnonymous,
            AllowVoteChange = source.AllowVoteChange,
            StartTime = Timestamp.FromDateTime(source.StartTime.ToUniversalTime()),
            EndTime = Timestamp.FromDateTime(source.EndTime.ToUniversalTime()),
            Status = source.VotingStatus switch
            {
                DataVotingStatus.Draft => VotingStatus.Draft,
                DataVotingStatus.Active => VotingStatus.Active,
                DataVotingStatus.Paused => VotingStatus.Paused,
                DataVotingStatus.Finished => VotingStatus.Finished,
                _ => (VotingStatus)0
            },
            CreatedAt = Timestamp.FromDateTime(source.CreatedAt.ToUniversalTime())
        };
    }
}