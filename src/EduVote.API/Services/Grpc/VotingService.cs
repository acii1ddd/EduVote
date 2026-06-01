using System.Security.Claims;
using EduVote.Application.Common;
using EduVote.Application.Votings.ApproveVoting;
using EduVote.Application.Votings.CastVote;
using EduVote.Application.Votings.CreateVoting;
using EduVote.Application.Votings.DeleteVoting;
using EduVote.Application.Votings.FinishVoting;
using EduVote.Application.Votings.GetVoting;
using EduVote.Application.Votings.GetVotings;
using EduVote.Application.Votings.GetVotingsCreatedByUser;
using EduVote.Application.Votings.GetVotingsForUser;
using EduVote.Application.Votings.PauseVoting;
using EduVote.Application.Votings.StartVoting;
using EduVote.Application.Votings.UpdateVoting;
using EduVote.API.Mappers;
using EduVote.API.Services.Tools;
using EduVote.API.Services.Tools.Votings;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;
using DbVoting = EduVote.DAL.Postgresql.Models.Voting;

namespace EduVote.API.Services.Grpc;

public class VotingService(
    IVotingRepository votingRepository,
    IVoteRepository voteRepository,
    ICandidateRepository candidateRepository,
    IVoteHashService voteHashService,
    IVotingResultRepository votingResultRepository,
    IBlockchainRecordRepository blockchainRecordRepository,
    ISender sender,
    ILogger<VotingService> logger)
    : Votings.VotingsBase
{
    public override async Task<VotingResponse> CreateVoting(
        CreateVotingRequest request, ServerCallContext context)
    {
        logger.LogInformation("[CreateVoting] Creating new voting with title '{Title}', type '{Type}', " +
            "start time '{StartTime}', end time '{EndTime}'", request.Title, request.Type, request.StartTime, request.EndTime);

        var httpUser = context.GetHttpContext().User;
        var callerRole = httpUser.FindFirst(ClaimTypes.Role)?.Value;
        var callerIdStr = httpUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var callerId = Guid.TryParse(callerIdStr, out var parsedCallerId)
            ? parsedCallerId
            : (Guid?)null;

        var command = new CreateVotingCommand(
            request.Title,
            request.Description,
            MapCreateVotingType(request.Type),
            request.IsAnonymous,
            request.AllowVoteChange,
            request.StartTime.ToDateTime().ToUniversalTime(),
            request.EndTime.ToDateTime().ToUniversalTime(),
            callerId,
            callerRole);

        try
        {
            var createdVoting = await sender.Send(command, context.CancellationToken);

            logger.LogInformation("[CreateVoting] Voting '{VotingId}' created with status '{Status}'",
                createdVoting.Id, createdVoting.Status);

            return MapCreateVotingResultToResponse(createdVoting);
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }
    }

    public override async Task<Empty> ApproveVoting(
        VotingActionRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");

        try
        {
            await sender.Send(new ApproveVotingCommand(votingId), context.CancellationToken);
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }

        logger.LogInformation("[ApproveVoting] Voting '{VotingId}' approved, moved to Draft", votingId);

        return new Empty();
    }

    public override async Task<VotingResponse> UpdateVoting(
        UpdateVotingRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");

        var command = new UpdateVotingCommand(
            votingId,
            request.Title,
            request.Description,
            MapCreateVotingType(request.Type),
            request.IsAnonymous,
            request.AllowVoteChange,
            request.StartTime.ToDateTime().ToUniversalTime(),
            request.EndTime.ToDateTime().ToUniversalTime());

        try
        {
            var updatedVoting = await sender.Send(command, context.CancellationToken);
            return MapUpdateVotingResultToResponse(updatedVoting);
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }
    }

    public override async Task<Empty> DeleteVoting(
        DeleteVotingRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");

        try
        {
            await sender.Send(new DeleteVotingCommand(votingId), context.CancellationToken);
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }

        return new Empty();
    }

    public override async Task<Empty> StartVoting(
        VotingActionRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");

        try
        {
            await sender.Send(new StartVotingCommand(votingId), context.CancellationToken);
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }

        return new Empty();
    }

    public override async Task<Empty> PauseVoting(
        VotingActionRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");

        try
        {
            await sender.Send(new PauseVotingCommand(votingId), context.CancellationToken);
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }

        return new Empty();
    }

    public override async Task<FinishVotingResponse> FinishVoting(
        VotingActionRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");

        FinishVotingResult result;
        try
        {
            result = await sender.Send(new FinishVotingCommand(votingId), context.CancellationToken);
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }

        return new FinishVotingResponse
        {
            VotingId = request.Id,
            Status = VotingStatus.Finished,
            TxHash = result.TxHash ?? string.Empty,
            EtherscanUrl = result.EtherscanUrl ?? string.Empty
        };
    }

    public override async Task<VotingResponse> GetVoting(
        GetVotingRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");

        try
        {
            var voting = await sender.Send(new GetVotingQuery(votingId), context.CancellationToken);
            return voting.MapToResponse();
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }
    }

    public override async Task<GetVotingsResponse> GetVotings(
        Empty request, ServerCallContext context)
    {
        var votings = await sender.Send(new GetVotingsQuery(), context.CancellationToken);

        var response = new GetVotingsResponse();
        response.Votings.AddRange(votings.MapToResponseList());

        return response;
    }

    // targeting votings for user - only those which are related to user's education units and above in hierarchy
    public override async Task<GetVotingsResponse> GetVotingsForUser(
        GetVotingsForUserRequest request, ServerCallContext context)
    {
        var userId = IdParser.ParseId(request.UserId, "User");

        try
        {
            var votings = await sender.Send(
                new GetVotingsForUserQuery(userId),
                context.CancellationToken);

            var response = new GetVotingsResponse();
            response.Votings.AddRange(votings.MapToResponseList());
            return response;
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }
    }

    public override async Task<GetVotingsResponse> GetVotingsCreatedByUser(
        Empty request, ServerCallContext context)
    {
        var userIdStr = context.GetHttpContext()
            .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdStr, out var userId))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "User identity not found in token."));

        var votings = await sender.Send(
            new GetVotingsCreatedByUserQuery(userId),
            context.CancellationToken);

        var createdResponse = new GetVotingsResponse();
        createdResponse.Votings.AddRange(votings.MapToResponseList());

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

        var command = new CastVoteCommand(
            votingId,
            userId,
            request.SelectedCandidateId,
            request.SelectedCandidateIds.ToList(),
            request.RatingAnswers.ToDictionary(x => x.Key, x => x.Value),
            request.TextAnswer);

        try
        {
            var result = await sender.Send(command, context.CancellationToken);

            logger.LogInformation("[CastVote] [{Timestamp}] User '{UserId}' vote saved " +
                "successfully in voting '{VotingId}'", DateTime.UtcNow, userId, votingId);

            return new CastVoteResponse
            {
                VoteId = result.VoteId.ToString(),
                VoteHash = result.VoteHash,
                VoteSalt = result.VoteSalt,
                CreatedAt = result.CreatedAt.ToUniversalTime().ToTimestamp()
            };
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
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
        
        var voteData = BuildVoteDataStruct(vote, voting.Type, candidates);

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

    public override async Task<VotingVerificationResponse> GetVerificationData(
        GetVotingRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");

        var voting = await GetVotingOrThrowAsync(votingId, context.CancellationToken);

        if (voting.Status != DbVotingStatus.Finished)
        {
            throw new RpcException(new Status(
                StatusCode.FailedPrecondition,
                "Voting is not finished yet."));
        }

        var existingResult = await votingResultRepository
            .GetByVotingIdAsync(votingId, context.CancellationToken);

        if (existingResult is null)
        {
            throw new RpcException(new Status(
                StatusCode.Unavailable,
                "Results will be available in the next minute."));
        }

        var votes = (await voteRepository
            .GetByVotingIdAsync(votingId, context.CancellationToken)).ToList();

        var response = new VotingVerificationResponse
        {
            VotingId = votingId.ToString(),
            ResultHash = existingResult.ResultHash,
            HashAlgorithm = "SHA-256",
            CombineMethod = "sort_ordinal_concat_no_separator",
            TotalVotes = votes.Count
        };

        response.VoteHashes.AddRange(votes.Select(v => v.VoteHash));

        return response;
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

    private static StatusCode MapStatusCode(ApplicationErrorType errorType) =>
        errorType switch
        {
            ApplicationErrorType.InvalidArgument => StatusCode.InvalidArgument,
            ApplicationErrorType.NotFound => StatusCode.NotFound,
            ApplicationErrorType.PermissionDenied => StatusCode.PermissionDenied,
            ApplicationErrorType.FailedPrecondition => StatusCode.FailedPrecondition,
            ApplicationErrorType.AlreadyExists => StatusCode.AlreadyExists,
            ApplicationErrorType.Unauthenticated => StatusCode.Unauthenticated,
            _ => StatusCode.Unknown
        };

    private static DbVotingType MapCreateVotingType(VotingType type) =>
        type switch
        {
            VotingType.SingleChoice => DbVotingType.SingleChoice,
            VotingType.MultipleChoice => DbVotingType.MultipleChoice,
            VotingType.Rating => DbVotingType.Rating,
            VotingType.OpenAnswer => DbVotingType.OpenAnswer,
            VotingType.Unspecified => throw new RpcException(
                new Status(StatusCode.InvalidArgument, "Voting type must be specified.")),
            _ => throw new RpcException(new Status(StatusCode.InvalidArgument, "Voting type is not valid."))
        };

    private static VotingResponse MapCreateVotingResultToResponse(CreateVotingResult result) =>
        new()
        {
            Id = result.Id.ToString(),
            Title = result.Title,
            Description = result.Description,
            Type = result.Type switch
            {
                DbVotingType.SingleChoice => VotingType.SingleChoice,
                DbVotingType.MultipleChoice => VotingType.MultipleChoice,
                DbVotingType.Rating => VotingType.Rating,
                DbVotingType.OpenAnswer => VotingType.OpenAnswer,
                _ => throw new RpcException(new Status(StatusCode.InvalidArgument, "Voting type is not valid."))
            },
            IsAnonymous = result.IsAnonymous,
            AllowVoteChange = result.AllowVoteChange,
            StartTime = result.StartTime.ToUniversalTime().ToTimestamp(),
            EndTime = result.EndTime.ToUniversalTime().ToTimestamp(),
            Status = result.Status switch
            {
                DbVotingStatus.Draft => VotingStatus.Draft,
                DbVotingStatus.Active => VotingStatus.Active,
                DbVotingStatus.Paused => VotingStatus.Paused,
                DbVotingStatus.Finished => VotingStatus.Finished,
                DbVotingStatus.PendingApproval => VotingStatus.PendingApproval,
                _ => throw new RpcException(new Status(StatusCode.InvalidArgument, "Voting status is not valid."))
            },
            CreatedAt = result.CreatedAt.ToUniversalTime().ToTimestamp(),
            CreatedById = result.CreatedById.ToString()
        };

    private static VotingResponse MapUpdateVotingResultToResponse(UpdateVotingResult result) =>
        new()
        {
            Id = result.Id.ToString(),
            Title = result.Title,
            Description = result.Description,
            Type = result.Type switch
            {
                DbVotingType.SingleChoice => VotingType.SingleChoice,
                DbVotingType.MultipleChoice => VotingType.MultipleChoice,
                DbVotingType.Rating => VotingType.Rating,
                DbVotingType.OpenAnswer => VotingType.OpenAnswer,
                _ => throw new RpcException(new Status(StatusCode.InvalidArgument, "Voting type is not valid."))
            },
            IsAnonymous = result.IsAnonymous,
            AllowVoteChange = result.AllowVoteChange,
            StartTime = result.StartTime.ToUniversalTime().ToTimestamp(),
            EndTime = result.EndTime.ToUniversalTime().ToTimestamp(),
            Status = result.Status switch
            {
                DbVotingStatus.Draft => VotingStatus.Draft,
                DbVotingStatus.Active => VotingStatus.Active,
                DbVotingStatus.Paused => VotingStatus.Paused,
                DbVotingStatus.Finished => VotingStatus.Finished,
                DbVotingStatus.PendingApproval => VotingStatus.PendingApproval,
                _ => throw new RpcException(new Status(StatusCode.InvalidArgument, "Voting status is not valid."))
            },
            CreatedAt = result.CreatedAt.ToUniversalTime().ToTimestamp(),
            CreatedById = result.CreatedById.ToString()
        };
    
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