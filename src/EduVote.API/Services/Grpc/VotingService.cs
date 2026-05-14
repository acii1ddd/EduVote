using System.Security.Claims;
using EduVote.API.Mappers;
using EduVote.API.Services.Tools;
using EduVote.API.Services.Tools.Votings;
using EduVote.API.Validators;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
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
    IBlockchainRecordRepository blockchainRecordRepository,
    VotingLifecycleService votingLifecycleService,
    ILogger<VotingService> logger)
    : Votings.VotingsBase
{
    public override async Task<VotingResponse> CreateVoting(
        CreateVotingRequest request, ServerCallContext context)
    {
        logger.LogInformation("[CreateVoting] Creating new voting with title '{Title}', type '{Type}', " +
            "start time '{StartTime}', end time '{EndTime}'", request.Title, request.Type, request.StartTime, request.EndTime);

        VotingValidator.ValidateDateRange(request.StartTime, request.EndTime);

        var voting = request.MapToEntity();

        // put in a separate method
        var httpUser = context.GetHttpContext().User;
        var callerRole = httpUser.FindFirst(ClaimTypes.Role)?.Value;
        var callerIdStr = httpUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(callerIdStr, out var callerId))
            voting.CreatedById = callerId;

        // Votings created by Teacher require Administrator approval before becoming active
        if (callerRole == Roles.Teacher)
            voting.Status = DbVotingStatus.PendingApproval;

        var createdVoting = await votingRepository
            .CreateAsync(voting, context.CancellationToken);

        logger.LogInformation("[CreateVoting] Voting '{VotingId}' created with status '{Status}'",
            createdVoting.Id, createdVoting.Status);

        return createdVoting.MapToResponse();
    }

    public override async Task<Empty> ApproveVoting(
        VotingActionRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");

        var voting = await GetVotingOrThrowAsync(votingId, context.CancellationToken);

        if (voting.Status != DbVotingStatus.PendingApproval)
        {
            throw new RpcException(new Status(
                StatusCode.FailedPrecondition,
                $"Voting {votingId} is not pending approval (current status: {voting.Status})."));
        }

        voting.Status = DbVotingStatus.Draft;

        await votingRepository.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("[ApproveVoting] Voting '{VotingId}' approved, moved to Draft", votingId);

        return new Empty();
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

    public override async Task<FinishVotingResponse> FinishVoting(
        VotingActionRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");

        var (txHash, etherscanUrl) = await votingLifecycleService
            .FinalizeVotingAsync(votingId, context.CancellationToken);

        return new FinishVotingResponse
        {
            VotingId = request.Id,
            Status = VotingStatus.Finished,
            TxHash = txHash ?? string.Empty,
            EtherscanUrl = etherscanUrl ?? string.Empty
        };
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

    // targeting votings for user - only those which are related to user's education units and above in hierarchy
    public override async Task<GetVotingsResponse> GetVotingsForUser(
        GetVotingsForUserRequest request, ServerCallContext context)
    {
        var userId = IdParser.ParseId(request.UserId, "User");

        var user = await userRepository
            .GetByIdWithEducationUnitsAsync(userId, context.CancellationToken);

        if (user is null)
            throw IdParser.CreateNotFoundException("User", request.UserId);

        var userUnitIds = user.UserEducationUnits
            .Select(ueu => ueu.EducationUnitId)
            .ToList();

        var allUnitIds = userUnitIds.Count > 0
            ? (await educationUnitRepository
                .GetAllParentIdsAsync(userUnitIds, context.CancellationToken))
                .ToList()
            : [];

        var votings = await votingRepository
            .GetVotingsForEducationUnitsAsync(allUnitIds, context.CancellationToken);

        var response = new GetVotingsResponse();
        response.Votings.AddRange(votings.MapToResponseList());

        return response;
    }

    public override async Task<GetVotingsResponse> GetVotingsCreatedByUser(
        Empty request, ServerCallContext context)
    {
        var userIdStr = context.GetHttpContext()
            .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdStr, out var userId))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "User identity not found in token."));

        var createdVotings = await votingRepository
            .GetByCreatedByAsync(userId, context.CancellationToken);

        var createdResponse = new GetVotingsResponse();
        createdResponse.Votings.AddRange(createdVotings.MapToResponseList());

        return createdResponse;
    }

    public override async Task<GetVotedVotingIdsResponse> GetVotedVotingIds(
        Empty request, ServerCallContext context)
    {
        var userIdStr = context.GetHttpContext()
            .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdStr, out var userId))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "User identity not found in token."));

        var ids = await voteRepository
            .GetVotedVotingIdsAsync(userId, context.CancellationToken);

        var response = new GetVotedVotingIdsResponse();
        response.VotingIds.AddRange(ids.Select(id => id.ToString()));
        return response;
    }

    public override async Task<CastVoteResponse> CastVote(
        CastVoteRequest request, ServerCallContext context)
    {
        var userIdStr = context.GetHttpContext()
            .User
            .FindFirst(ClaimTypes.NameIdentifier)?
            .Value!;
        
        var votingId = IdParser.ParseId(request.VotingId, "Voting");
        var userId = IdParser.ParseId(userIdStr, "User");

        logger.LogInformation("[CastVote] [{Timestamp}] User '{UserId}' attempting to " +
            "cast vote in voting {VotingId}", DateTime.UtcNow, userId, votingId);
        
        var voting = await GetVotingOrThrowAsync(votingId, context.CancellationToken);

        if (voting.Status == DbVotingStatus.Finished)
        {
            logger.LogWarning(
                "[CastVote] [{Timestamp}] User '{UserId}' attempted to vote in finished voting '{VotingId}'",
                DateTime.UtcNow,
                userId,
                votingId
            );

            throw new RpcException(new Status(
                StatusCode.FailedPrecondition,
                "Voting is already finished."));
        }
        
        var user = await userRepository
            .GetByIdWithEducationUnitsAsync(userId, context.CancellationToken);
        
        if (user is null)
        {
            throw IdParser.CreateNotFoundException("User", userId.ToString());
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
            logger.LogInformation("[CastVote] [{Timestamp}] Voting '{VotingId}' is public, " +
                "access allowed", DateTime.UtcNow, votingId);
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
                logger.LogWarning("[CastVote] [{Timestamp}] User '{UserId}' does not " +
                    "have access to voting {VotingId}", DateTime.UtcNow, userId, votingId);
                
                throw new RpcException(new Status(
                    StatusCode.PermissionDenied,
                    "User does not have access to vote in this voting."));
            }
            
            logger.LogInformation("[CastVote] [{Timestamp}] User '{UserId}' has access " +
                "to restricted voting {VotingId}", DateTime.UtcNow, userId, votingId);
        }
        
        var existingVote = await voteRepository
            .GetUserVoteAsync(votingId, userId, context.CancellationToken);

        if (existingVote is not null && !voting.AllowVoteChange)
        {
            logger.LogWarning("[CastVote] [{Timestamp}] User '{UserId}' already voted in voting '{VotingId}' " +
                "and vote change not allowed", DateTime.UtcNow, userId, votingId);
            
            throw new RpcException(new Status(
                StatusCode.AlreadyExists,
                "User has already voted in this voting and vote change is not allowed."));
        }

        var candidates = (
            await candidateRepository.GetByVotingIdAsync(votingId, context.CancellationToken)
        ).ToList();

        VoteValidator.ValidateVote(request, voting, candidates);

        var salt = Guid.NewGuid().ToString("N");
        var voteHash = voteHashService
            .GenerateVoteHash(votingId, userId, voting.Type, salt, request);

        // Update existing vote way
        if (existingVote is not null)
        {
            UpdateExistingVote(existingVote, request, voteHash, salt, voting.Type);
            
            await voteRepository.SaveChangesAsync(context.CancellationToken);
            
            logger.LogInformation("[CastVote] [{Timestamp}] User '{UserId}' vote updated " +
                "successfully in voting '{VotingId}'", DateTime.UtcNow, userId, votingId);
            
            return existingVote.MapToResponse();
        }

        // Create new vote way
        var newVote = CreateNewVote(votingId, userId, request, voteHash, salt, voting.Type);
        
        var createdVote = await voteRepository
            .CreateAsync(newVote, context.CancellationToken);

        logger.LogInformation("[CastVote] [{Timestamp}] User '{UserId}' vote created " +
            "successfully in voting '{VotingId}'", DateTime.UtcNow, userId, votingId);
        
        return createdVote.MapToResponse();
    }
    
    private static void UpdateExistingVote(
        Vote vote,
        CastVoteRequest request,
        string voteHash,
        string salt,
        DbVotingType votingType)
    {
        vote.VoteHash = voteHash;
        vote.VoteSalt = salt;

        PopulateVoteData(vote, request, votingType);
    }

    private static Vote CreateNewVote(
        Guid votingId,
        Guid userId,
        CastVoteRequest request,
        string voteHash,
        string salt,
        DbVotingType votingType)
    {
        var vote = new Vote
        {
            Id = Guid.NewGuid(),
            VotingId = votingId,
            UserId = userId,
            VoteHash = voteHash,
            VoteSalt = salt,
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

    public override async Task<MyVoteResponse> GetMyVote(
        GetVotingRequest request, ServerCallContext context)
    {
        var votingId  = IdParser.ParseId(request.Id, "Voting");
        
        var userIdStr = context.GetHttpContext()
            .User.FindFirst(ClaimTypes.NameIdentifier)?
            .Value;
        
        if (!Guid.TryParse(userIdStr, out var userId))
            throw new RpcException(new Status(
                StatusCode.Unauthenticated, "Invalid user identity."));

        var voting = await GetVotingOrThrowAsync(votingId, context.CancellationToken);

        var vote = await voteRepository
            .GetUserVoteAsync(votingId, userId, context.CancellationToken);
        
        if (vote is null)
            throw new RpcException(new Status(
                StatusCode.NotFound, "You have not voted in this voting.")
            );

        if (vote.UserId != userId)
        {
            throw new RpcException(new Status(
                StatusCode.PermissionDenied,
                "This vote does not belong to the current user."
            ));
        }
        
        var candidates = (await candidateRepository
            .GetByVotingIdAsync(votingId, context.CancellationToken)).ToList();

        var hashInput = voteHashService
            .BuildHashInput(vote, voting.Type);
        
        var voteData  = BuildVoteDataStruct(vote, voting.Type, candidates);

        return new MyVoteResponse
        {
            VoteId = vote.Id.ToString(),
            VoteHash = vote.VoteHash,
            VoteSalt = vote.VoteSalt,
            HashInput = hashInput,
            VoteData = voteData
        };
    }

    private static Struct BuildVoteDataStruct(Vote vote, DbVotingType votingType, 
        List<Candidate> candidates)
    {
        object data = votingType switch
        {
            DbVotingType.SingleChoice => new
            {
                type = "SingleChoice",
                candidate = candidates
                    .Where(c => c.Id == vote.CandidateId)
                    .Select(c => new { id = c.Id.ToString(), name = c.Name })
                    .FirstOrDefault()
            },
            DbVotingType.MultipleChoice => new
            {
                type = "MultipleChoice",
                candidates = vote.GetSelectedCandidateIds()
                    .Select(id => candidates.FirstOrDefault(c => c.Id == id))
                    .Where(c => c is not null)
                    .Select(c => new { id = c!.Id.ToString(), name = c.Name })
                    .ToList()
            },
            DbVotingType.Rating => new
            {
                type = "Rating",
                ratings = vote.GetRatingAnswers()
                    .Select(kvp =>
                    {
                        var candidate = candidates.FirstOrDefault(c => c.Id == kvp.Key);
                        return new { id = kvp.Key.ToString(), name = candidate?.Name ?? "Unknown", rating = kvp.Value };
                    })
                    .ToList()
            },
            DbVotingType.OpenAnswer => new
            {
                type = "OpenAnswer",
                textAnswer = vote.TextAnswer ?? string.Empty
            },
            _ => new { type = "Unknown" }
        };

        return Struct.Parser.ParseJson(JsonSerializer.Serialize(data));
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
        
        logger.LogInformation("Results for voting {VotingId} requested.", votingId);

        var response = MapVotingResultToResponse(existingResult);

        var blockchainRecord = await blockchainRecordRepository
            .GetByVotingResultIdAsync(existingResult.Id, context.CancellationToken);

        if (blockchainRecord is not null)
        {
            response.TxHash = blockchainRecord.TransactionHash;
            response.EtherscanUrl = $"https://sepolia.etherscan.io/tx/{blockchainRecord.TransactionHash}";
        }

        return response;
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
}