using EduVote.API.Mappers;
using EduVote.DAL.Postgresql.Repositories;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;

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
        var votingId = IdParser.ParseId(request.Id, "Voting");
        ValidateDateRange(request.StartTime, request.EndTime);
        
        var existingVoting = await votingRepository
            .GetByIdAsync(votingId, context.CancellationToken);
        
        if (existingVoting is null)
        {
            throw IdParser.CreateNotFoundException("Voting", request.Id);
        }

        var updatedVoting = request.MapToEntity();
        
        existingVoting.Title = updatedVoting.Title;
        existingVoting.Description = updatedVoting.Description;
        existingVoting.Type = updatedVoting.Type;
        existingVoting.IsAnonymous = updatedVoting.IsAnonymous;
        existingVoting.AllowVoteChange = updatedVoting.AllowVoteChange;
        existingVoting.StartTime = updatedVoting.StartTime;
        existingVoting.EndTime = updatedVoting.EndTime;

        await votingRepository
            .SaveChangesAsync(context.CancellationToken);

        return existingVoting.MapToResponse();
    }

    public override async Task<Empty> DeleteVoting(DeleteVotingRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");

        var existingVoting = await votingRepository.GetByIdAsync(votingId, context.CancellationToken);
        
        if (existingVoting is null)
        {
            throw IdParser.CreateNotFoundException("Voting", request.Id);
        }
        
        await votingRepository
            .DeleteAsync(existingVoting, context.CancellationToken);

        return new Empty();
    }

    public override async Task<VotingResponse> StartVoting(VotingActionRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");

        return await ChangeStatus(
            votingId,
            DbVotingStatus.Active,
            [DbVotingStatus.Draft, DbVotingStatus.Paused],
            context.CancellationToken
        );
    }

    public override async Task<VotingResponse> PauseVoting(VotingActionRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");

        return await ChangeStatus(
            votingId,
            DbVotingStatus.Paused,
            [DbVotingStatus.Active],
            context.CancellationToken);
    }

    public override async Task<VotingResponse> FinishVoting(VotingActionRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");

        return await ChangeStatus(
            votingId,
            DbVotingStatus.Finished,
            [DbVotingStatus.Active, DbVotingStatus.Paused],
            context.CancellationToken);
    }


    public override async Task<VotingResponse> GetVoting(GetVotingRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");

        var existingVoting = await votingRepository
            .GetByIdAsync(votingId, context.CancellationToken);
        
        if (existingVoting is null)
        {
            throw IdParser.CreateNotFoundException("Voting", request.Id);
        }
        
        return existingVoting.MapToResponse();
    }

    // todo пагинация
    public override async Task<GetVotingsResponse> GetVotings(Empty request, ServerCallContext context)
    {
        var existingVotings = await votingRepository
            .GetAllAsync(context.CancellationToken);
        
        var response = new GetVotingsResponse();
        response.Votings.AddRange(existingVotings.MapToResponseList());
        
        return response;
    }

    private async Task<VotingResponse> ChangeStatus(
        Guid votingId,
        DbVotingStatus newStatus,
        IReadOnlyCollection<DbVotingStatus> allowedCurrentStatuses,
        CancellationToken cancellationToken)
    {
        var existingVoting = await votingRepository
            .GetByIdAsync(votingId, cancellationToken);
        
        if (existingVoting is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Voting with id {votingId} was not found."));
        }

        if (!allowedCurrentStatuses.Contains(existingVoting.VotingStatus))
        {
            throw new RpcException(new Status(
                StatusCode.FailedPrecondition,
                $"Voting {votingId} cannot be moved from {existingVoting.VotingStatus} to {newStatus}."));
        }

        existingVoting.VotingStatus = newStatus;
        
        await votingRepository
            .SaveChangesAsync(cancellationToken);

        return existingVoting.MapToResponse();
    }
    
    // todo вынести в домен
    private static void ValidateDateRange(Timestamp? startTime, Timestamp? endTime)
    {
        if (startTime is null || endTime is null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "start_time and end_time are required."));
        }

        var startDate = startTime.ToDateTime();
        var endDate = endTime.ToDateTime();

        var diff = endDate - startDate;

        if (diff < TimeSpan.FromHours(1))
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "end_time must be at least 1 hour greater than start_time."));
        }
        
        if (startDate > endDate)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "start_time must be less than end_time."));
        }
    }
}