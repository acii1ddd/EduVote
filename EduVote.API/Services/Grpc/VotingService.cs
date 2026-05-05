using System.Text.Json;
using EduVote.API.Mappers;
using EduVote.API.Services.Tools;
using EduVote.API.Validators;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Repositories;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.API.Services.Grpc;

public class VotingService(
    IVotingRepository votingRepository,
    IVoteRepository voteRepository,
    IUserRepository userRepository,
    ICandidateRepository candidateRepository,
    IVotingTargetRepository votingTargetRepository,
    IVoteHashService voteHashService)
    : Votings.VotingsBase
{
    public override async Task<VotingResponse> CreateVoting(CreateVotingRequest request, ServerCallContext context)
    {
        VotingValidator.ValidateDateRange(request.StartTime, request.EndTime);

        var voting = request.MapToEntity();

        var createdVoting = await votingRepository
            .CreateAsync(voting, context.CancellationToken);
        
        return createdVoting.MapToResponse();
    }

    public override async Task<VotingResponse> UpdateVoting(UpdateVotingRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");
        VotingValidator.ValidateDateRange(request.StartTime, request.EndTime);
        
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

    public override async Task<CastVoteResponse> CastVote(
        CastVoteRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.VotingId, "Voting");
        var userId = IdParser.ParseId(request.UserId, "User");

        var voting = await votingRepository
            .GetByIdAsync(votingId, context.CancellationToken);
        
        if (voting is null)
        {
            throw IdParser.CreateNotFoundException("Voting", request.VotingId);
        }

        var user = await userRepository
            .GetByIdWithEducationUnitsAsync(userId, context.CancellationToken);
        
        if (user is null)
        {
            throw IdParser.CreateNotFoundException("User", request.UserId);
        }

        // User's educationUnits
        var userEducationUnitIds = user.UserEducationUnits
            .Select(ueu => ueu.EducationUnitId)
            .ToList();

        var votingTargets = await votingTargetRepository
            .GetByVotingIdAsync(votingId, context.CancellationToken);

        // Voting's educationUnits
        var targetEducationUnitIds = votingTargets
            .Select(vt => vt.EducationUnitId)
            .ToList();

        // either voting has no targets -> all users access
        // or users targets has at least one common with voting targets
        var isPublicVoting = targetEducationUnitIds.Count == 0;
        var hasMatchingUnit = userEducationUnitIds
            .Any(unitId => targetEducationUnitIds.Contains(unitId));
        
        // access denied
        if (!isPublicVoting && !hasMatchingUnit)
        {
            throw new RpcException(new Status(
                StatusCode.PermissionDenied,
                "User does not have access to vote in this voting."));
        }
        
        // todo голосование для университета должно быть видно всем пользователям
        
        var existingVote = await voteRepository
            .GetUserVoteAsync(votingId, userId, context.CancellationToken);

        if (existingVote is not null && !voting.AllowVoteChange)
        {
            throw new RpcException(new Status(
                StatusCode.AlreadyExists,
                "User has already voted in this voting and vote change is not allowed."));
        }

        var candidates = (
            await candidateRepository.GetByVotingIdAsync(votingId, context.CancellationToken)
        ).ToList();

        VoteValidator.ValidateVote(request, voting, candidates);

        var voteHash = voteHashService
            .GenerateHash(votingId, userId, voting.Type, request);

        // update existing vote way
        if (existingVote is not null)
        {
            UpdateExistingVote(existingVote, request, voteHash, voting.Type);
            
            await voteRepository.SaveChangesAsync(context.CancellationToken);
            
            return existingVote.MapToResponse();
        }

        // create new vote way
        var newVote = CreateNewVote(votingId, userId, request, voteHash, voting.Type);
        
        var createdVote = await voteRepository
            .CreateAsync(newVote, context.CancellationToken);

        return createdVote.MapToResponse();
    }
    
    private static void UpdateExistingVote(
        Vote vote,
        CastVoteRequest request,
        string voteHash,
        DbVotingType votingType)
    {
        vote.VoteHash = voteHash;
        
        PopulateVoteData(vote, request, votingType);
    }

    private static Vote CreateNewVote(
        Guid votingId,
        Guid userId,
        CastVoteRequest request,
        string voteHash,
        DbVotingType votingType)
    {
        var vote = new Vote
        {
            Id = Guid.NewGuid(),
            VotingId = votingId,
            UserId = userId,
            VoteHash = voteHash,
        };

        PopulateVoteData(vote, request, votingType);
        return vote;
    }
    

    private static void PopulateVoteData(Vote vote, CastVoteRequest request, DbVotingType votingType)
    {
        vote.CandidateId = null;
        vote.SelectedCandidateIds = null;
        vote.RatingAnswers = null;
        vote.TextAnswer = null;

        switch (votingType)
        {
            case DbVotingType.SingleChoice:
                vote.CandidateId = Guid.Parse(request.SelectedCandidateId);
                break;
            case DbVotingType.MultipleChoice:
                vote.SelectedCandidateIds = JsonSerializer.Serialize(
                    request.SelectedCandidateIds
                );
                break;
            case DbVotingType.Rating:
                vote.RatingAnswers = JsonSerializer.Serialize(
                    request.RatingAnswers
                );
                break;
            case DbVotingType.OpenAnswer:
                vote.TextAnswer = request.TextAnswer;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(votingType), votingType, "Unknown vote type");
        }
    }
}