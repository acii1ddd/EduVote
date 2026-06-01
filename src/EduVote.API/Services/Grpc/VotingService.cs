using System.Security.Claims;
using EduVote.Application.Common;
using EduVote.Application.Votings.ApproveVoting;
using EduVote.Application.Votings.CastVote;
using EduVote.Application.Votings.CreateVoting;
using EduVote.Application.Votings.DeleteVoting;
using EduVote.Application.Votings.FinishVoting;
using EduVote.Application.Votings.GetVoting;
using EduVote.Application.Votings.GetVotings;
using EduVote.Application.Votings.GetResults;
using EduVote.Application.Votings.GetVerificationData;
using EduVote.Application.Votings.GetMyVote;
using EduVote.Application.Votings.GetVotedVotingIds;
using EduVote.Application.Votings.GetVotingsCreatedByUser;
using EduVote.Application.Votings.GetVotingsForUser;
using EduVote.Application.Votings.PauseVoting;
using EduVote.Application.Votings.StartVoting;
using EduVote.Application.Votings.UpdateVoting;
using EduVote.API.Mappers;
using EduVote.API.Services.Tools;
using MediatR;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;
using DbVoting = EduVote.DAL.Postgresql.Models.Voting;

namespace EduVote.API.Services.Grpc;

public class VotingService(
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

        var ids = await sender.Send(
            new GetVotedVotingIdsQuery(userId),
            context.CancellationToken);

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
        var votingId = IdParser.ParseId(request.Id, "Voting");

        var userIdStr = context.GetHttpContext()
            .User.FindFirst(ClaimTypes.NameIdentifier)?
            .Value;

        if (!Guid.TryParse(userIdStr, out var userId))
        {
            throw new RpcException(new Status(
                StatusCode.Unauthenticated, "Invalid user identity."));
        }

        try
        {
            var result = await sender.Send(
                new GetMyVoteQuery(votingId, userId),
                context.CancellationToken);

            return new MyVoteResponse
            {
                VoteId = result.VoteId.ToString(),
                VoteHash = result.VoteHash,
                VoteSalt = result.VoteSalt,
                HashInput = result.HashInput,
                VoteData = Struct.Parser.ParseJson(result.VoteDataJson)
            };
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }
    }

    public override async Task<VotingVerificationResponse> GetVerificationData(
        GetVotingRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");

        try
        {
            var result = await sender.Send(
                new GetVerificationDataQuery(votingId),
                context.CancellationToken);

            var response = new VotingVerificationResponse
            {
                VotingId = result.VotingId.ToString(),
                ResultHash = result.ResultHash,
                HashAlgorithm = "SHA-256",
                CombineMethod = "sort_ordinal_concat_no_separator",
                TotalVotes = result.TotalVotes
            };

            response.VoteHashes.AddRange(result.VoteHashes);
            return response;
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }
    }

    public override async Task<VotingResultsResponse> GetResults(
        GetVotingRequest request, ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.Id, "Voting");

        try
        {
            var result = await sender.Send(
                new GetResultsQuery(votingId),
                context.CancellationToken);

            logger.LogInformation("Results for voting {VotingId} requested.", votingId);

            return MapVotingResultToResponse(result);
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }
    }

    private VotingResultsResponse MapVotingResultToResponse(GetResultsResult result)
    {
        var response = new VotingResultsResponse
        {
            VotingId = result.VotingId.ToString(),
            ResultHash = result.ResultHash,
            CalculatedAt = result.CalculatedAt.ToUniversalTime().ToTimestamp(),
            TotalVotes = result.TotalVotes,
            TxHash = result.TxHash ?? string.Empty,
            EtherscanUrl = result.EtherscanUrl ?? string.Empty
        };

        if (string.IsNullOrEmpty(result.ResultData)) return response;
        
        logger.LogInformation("[MapVotingResultToResponse] ResultData " +
            "for voting is {Result}. ", result.ResultData);
        
        try
        {
            var jsonDoc = JsonDocument.Parse(result.ResultData);
            
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
            ApplicationErrorType.Unavailable => StatusCode.Unavailable,
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
}