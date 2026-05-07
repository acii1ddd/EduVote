using EduVote.API.Mappers;
using EduVote.API.Services.Tools;
using EduVote.API.Validators;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Repositories;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;
using DbVoting = EduVote.DAL.Postgresql.Models.Voting;

namespace EduVote.API.Services.Grpc;

public class VotingService(
    IVotingRepository votingRepository,
    IVoteRepository voteRepository,
    IUserRepository userRepository,
    ICandidateRepository candidateRepository,
    IVotingTargetRepository votingTargetRepository,
    IVoteHashService voteHashService,
    IEducationUnitRepository educationUnitRepository,
    IVotingResultRepository votingResultRepository,
    VotingLifecycleService votingLifecycleService, 
    ILogger<VotingService> logger)
    : Votings.VotingsBase
{
    public override async Task<VotingResponse> CreateVoting(
        CreateVotingRequest request, ServerCallContext context)
    {
        VotingValidator.ValidateDateRange(request.StartTime, request.EndTime);

        var voting = request.MapToEntity();

        var createdVoting = await votingRepository
            .CreateAsync(voting, context.CancellationToken);
        
        return createdVoting.MapToResponse();
    }

    public override async Task<VotingResponse> UpdateVoting(
        UpdateVotingRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");
        VotingValidator.ValidateDateRange(request.StartTime, request.EndTime);
        
        var existingVoting = await GetVotingOrThrowAsync(votingId, context.CancellationToken);

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

    public override async Task<Empty> DeleteVoting(
        DeleteVotingRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");

        var voting = await GetVotingOrThrowAsync(votingId, context.CancellationToken);
        
        await votingRepository
            .DeleteAsync(voting, context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> StartVoting(
        VotingActionRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");
        
        await votingLifecycleService.ChangeStatusAsync(
            votingId,
            DbVotingStatus.Active,
            [DbVotingStatus.Draft, DbVotingStatus.Paused],
            context.CancellationToken
        );

        return new Empty();
    }

    public override async Task<Empty> PauseVoting(
        VotingActionRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");

        await votingLifecycleService.ChangeStatusAsync(
            votingId,
            DbVotingStatus.Paused,
            [DbVotingStatus.Active],
            context.CancellationToken
        );

        return new Empty();
    }

    public override async Task<Empty> FinishVoting(
        VotingActionRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");

        await votingLifecycleService
            .FinalizeVotingAsync(votingId, context.CancellationToken);
        
        return new Empty();
    }

    public override async Task<VotingResponse> GetVoting(
        GetVotingRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");

        var voting = await GetVotingOrThrowAsync(votingId, context.CancellationToken);
        
        return voting.MapToResponse();
    }

    // todo пагинация
    public override async Task<GetVotingsResponse> GetVotings(
        Empty request, ServerCallContext context)
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

        var voting = await GetVotingOrThrowAsync(votingId, context.CancellationToken);

        var user = await userRepository
            .GetByIdWithEducationUnitsAsync(userId, context.CancellationToken);
        
        if (user is null)
        {
            throw IdParser.CreateNotFoundException("User", request.UserId);
        }

        // Get voting targets (education units where this voting is restricted to)
        var votingTargets = await votingTargetRepository
            .GetByVotingIdAsync(votingId, context.CancellationToken);

        var targetEducationUnitIds = votingTargets
            .Select(vt => vt.EducationUnitId)
            .ToList();

        // If voting has no targets -> it's public (all users can vote)
        var isPublicVoting = targetEducationUnitIds.Count == 0;

        if (isPublicVoting)
        {
            // Public voting - access allowed for all users
        }
        else
        {
            // Voting has restrictions - check if user has access
            var userEducationUnitIds = user.UserEducationUnits
                .Select(ueu => ueu.EducationUnitId)
                .ToList();

            // Get all parents for user's education units
            // E.g., Group 1-SO-1 -> Course 2 -> Speciality IS -> Faculty IT -> University
            var userAllUnitsWithAncestorIds = (
                    await educationUnitRepository
                        .GetAllParentIdsAsync(userEducationUnitIds, context.CancellationToken)
            )
            .ToList();
            
            // Check if user has access: 
            // User must have at least one matching unit with voting target
            var hasAccess = targetEducationUnitIds
                .Any(targetId => userAllUnitsWithAncestorIds.Contains(targetId));

            if (!hasAccess)
            {
                throw new RpcException(new Status(
                    StatusCode.PermissionDenied,
                    "User does not have access to vote in this voting."));
            }
        }
        
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

        // Update existing vote way
        if (existingVote is not null)
        {
            UpdateExistingVote(existingVote, request, voteHash, voting.Type);
            
            await voteRepository.SaveChangesAsync(context.CancellationToken);
            
            return existingVote.MapToResponse();
        }

        // Create new vote way
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
    

    private static void PopulateVoteData(
        Vote vote, 
        CastVoteRequest request, 
        DbVotingType votingType)
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

    public override async Task<VotingResultsResponse> GetResults(
        GetVotingRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");
        
        var voting = await GetVotingOrThrowAsync(votingId, context.CancellationToken);

        // Check for voting is finished
        if (voting.Status != DbVotingStatus.Finished)
        {
            throw new RpcException(new Status(
                StatusCode.FailedPrecondition,
                "Voting is not finished yet."));
        }
        
        var existingResult = await votingResultRepository
            .GetByVotingIdAsync(votingId, context.CancellationToken);
        
        // Bg job will calculate the results soon
        if (existingResult is null)
        {
            throw new RpcException(new Status(
                StatusCode.Unavailable,
                "Results will be available in the next minute."));
        }
        
        logger.LogInformation("Results for voting {VotingId} requested. " +
            "Existing result is not null!:", votingId);
        
        return MapVotingResultToResponse(existingResult);
    }

    private VotingResultsResponse MapVotingResultToResponse(VotingResult votingResult)
    {
        var response = new VotingResultsResponse
        {
            VotingId = votingResult.VotingId.ToString(),
            ResultHash = votingResult.ResultHash,
            CalculatedAt = votingResult.CalculatedAt.ToUniversalTime().ToTimestamp(),
            TotalVotes = votingResult.TotalVotes
        };

        // Deserialize result data
        if (string.IsNullOrEmpty(votingResult.ResultData)) return response;
        
        logger.LogInformation("[MapVotingResultToResponse] ResultData " +
            "for voting is {Result}. ", votingResult.ResultData);
        
        try
        {
            var jsonDoc = JsonDocument.Parse(votingResult.ResultData);
            
            foreach (var property in jsonDoc.RootElement.EnumerateObject())
            {
                var structValue = Struct.Parser.ParseJson(property.Value.GetRawText());
            
                response.Results.Add(property.Name, structValue);
            }

            logger.LogInformation("[MapVotingResultToResponse] Results was mapped successfully");
        }
        catch (Exception ex)
        {
            logger.LogCritical("[MapVotingResultToResponse] Deserialization fails, " +
                "results will be empty, error: {ErrorMessage}", ex.Message);
        }

        return response;
    }
    
    private async Task<DbVoting> GetVotingOrThrowAsync(
        Guid votingId,
        CancellationToken cancellationToken)
    {
        var existingVoting = await votingRepository
            .GetByIdAsync(votingId, cancellationToken);

        if (existingVoting is null)
        {
            throw IdParser.CreateNotFoundException("Voting", votingId.ToString());
        }

        return existingVoting;
    }
    
    // public override async Task<VotingStatsResponse> GetStats(
    //     GetVotingRequest request, ServerCallContext context)
    // {
    //     var votingId = IdParser.ParseId(request.Id, "Voting");
    //
    //     var voting = await votingRepository
    //         .GetByIdAsync(votingId, context.CancellationToken);
    //     
    //     if (voting is null)
    //     {
    //         throw IdParser.CreateNotFoundException("Voting", request.Id);
    //     }
    //
    //     var votes = await voteRepository
    //         .GetByVotingIdAsync(votingId, context.CancellationToken);
    //     
    //     var candidates = await candidateRepository
    //         .GetByVotingIdAsync(votingId, context.CancellationToken);
    //
    //     var candidatesList = candidates.ToList();
    //
    //     var totalVotes = votes.Count();
    //     var participationPercentage = candidatesList.Count != 0
    //         ? (totalVotes * 100) / Math.Max(1, candidatesList.Count) 
    //         : 0;
    //
    //     var candidateStats = new List<CandidateStats>();
    //     var voteDistribution = new Dictionary<string, int>();
    //
    //     foreach (var candidate in candidatesList)
    //     {
    //         var candidateVoteCount = votes.Count(v => v.CandidateId == candidate.Id);
    //         var percentage = totalVotes > 0 ? (candidateVoteCount * 100.0) / totalVotes : 0;
    //
    //         candidateStats.Add(new CandidateStats
    //         {
    //             CandidateId = candidate.Id.ToString(),
    //             CandidateTitle = candidate.Title,
    //             VoteCount = candidateVoteCount,
    //             Percentage = percentage
    //         });
    //
    //         voteDistribution[candidate.Id.ToString()] = candidateVoteCount;
    //     }
    //
    //     var response = new VotingStatsResponse
    //     {
    //         VotingId = votingId.ToString(),
    //         TotalVotes = totalVotes,
    //         ParticipationPercentage = participationPercentage
    //     };
    //
    //     response.CandidateStats.AddRange(candidateStats);
    //     response.VoteDistribution.Add(voteDistribution);
    //
    //     return response;
    // }
    //
    // public override async Task<VotingVerificationResponse> GetVerification(
    //     GetVotingRequest request, ServerCallContext context)
    // {
    //     var votingId = IdParser.ParseId(request.Id, "Voting");
    //
    //     var voting = await votingRepository
    //         .GetByIdAsync(votingId, context.CancellationToken);
    //     
    //     if (voting is null)
    //     {
    //         throw IdParser.CreateNotFoundException("Voting", request.Id);
    //     }
    //
    //     var votingResult = await votingResultRepository.GetByVotingIdAsync(votingId, context.CancellationToken);
    //     if (votingResult is null)
    //     {
    //         throw new RpcException(new Status(StatusCode.NotFound, "Voting results not yet calculated"));
    //     }
    //
    //     var response = new VotingVerificationResponse
    //     {
    //         VotingId = votingId.ToString(),
    //         ResultHash = votingResult.ResultHash,
    //         VerifiedAt = votingResult.CalculatedAt.ToUniversalTime().ToTimestamp()
    //     };
    //
    //     // Get blockchain record if exists
    //     var blockchainRecord = await blockchainRecordRepository.GetByVotingIdAsync(votingId, context.CancellationToken);
    //     if (blockchainRecord is not null)
    //     {
    //         response.Blockchain = new BlockchainVerification
    //         {
    //             TransactionHash = blockchainRecord.TransactionHash ?? string.Empty,
    //             VotesHash = blockchainRecord.VotesHash ?? string.Empty,
    //             BlockNumber = blockchainRecord.BlockNumber ?? 0,
    //             Network = blockchainRecord.Network,
    //             Status = blockchainRecord.Status,
    //             ErrorMessage = blockchainRecord.ErrorMessage ?? string.Empty
    //         };
    //     }
    //
    //     return response;
    // }
}