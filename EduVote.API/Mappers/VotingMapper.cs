using DbVotingStatus  = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;
using DbVoting = EduVote.DAL.Postgresql.Models.Voting;

namespace EduVote.API.Mappers;

public static class VotingMapper
{
    private static readonly TypeAdapterConfig Config = new();

    static VotingMapper()
    {
        // Сопоставление между DAL Voting и gRPC VotingResponse
        Config.NewConfig<DbVoting, VotingResponse>()
            .Map(dest => dest.Id, src => src.Id.ToString())
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Type, src => MapToGrpcType(src.Type))
            .Map(dest => dest.IsAnonymous, src => src.IsAnonymous)
            .Map(dest => dest.AllowVoteChange, src => src.AllowVoteChange)
            .Map(dest => dest.StartTime, src => Timestamp.FromDateTime(src.StartTime.ToUniversalTime()))
            .Map(dest => dest.EndTime, src => Timestamp.FromDateTime(src.EndTime.ToUniversalTime()))
            .Map(dest => dest.Status, src => MapToGrpcStatus(src.Status))
            .Map(dest => dest.CreatedAt, src => Timestamp.FromDateTime(src.CreatedAt.ToUniversalTime()));

        // Обратное сопоставление (если понадобится)
        Config.NewConfig<CreateVotingRequest, DbVoting>()
            .Map(dest => dest.Id, src => Guid.NewGuid())
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Type, src => MapToDbType(src.Type))
            .Map(dest => dest.IsAnonymous, src => src.IsAnonymous)
            .Map(dest => dest.AllowVoteChange, src => src.AllowVoteChange)
            .Map(dest => dest.StartTime, src => src.StartTime.ToDateTime().ToUniversalTime())
            .Map(dest => dest.EndTime, src => src.EndTime.ToDateTime().ToUniversalTime())
            .Map(dest => dest.Status, _ => DbVotingStatus.Draft); // Создается как Draft

        Config.NewConfig<UpdateVotingRequest, DbVoting>()
            .Map(dest => dest.Id, src => Guid.Parse(src.Id))
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Type, src => MapToDbType(src.Type))
            .Map(dest => dest.IsAnonymous, src => src.IsAnonymous)
            .Map(dest => dest.AllowVoteChange, src => src.AllowVoteChange)
            .Map(dest => dest.StartTime, src => src.StartTime.ToDateTime().ToUniversalTime())
            .Map(dest => dest.EndTime, src => src.EndTime.ToDateTime().ToUniversalTime());
        // VotingStatus не меняется при update, не маппим
    }

    public static VotingResponse MapToResponse(this DbVoting source)
    {
        return source.Adapt<VotingResponse>(Config);
    }

    public static DbVoting MapToEntity(this CreateVotingRequest request)
    {
        return request.Adapt<DbVoting>(Config);
    }

    public static DbVoting MapToEntity(this UpdateVotingRequest request)
    {
        return request.Adapt<DbVoting>(Config);
    }

    private static DbVotingType MapToDbType(VotingType type)
    {
        return type switch
        {
            VotingType.SingleChoice => DbVotingType.SingleChoice,
            VotingType.MultipleChoice => DbVotingType.MultipleChoice,
            VotingType.Rating => DbVotingType.Rating,
            VotingType.OpenAnswer => DbVotingType.OpenAnswer,
            VotingType.Unspecified => throw new RpcException(
                new Status(StatusCode.InvalidArgument, "Voting type must be specified.")),
            _ => throw new RpcException(new Status(StatusCode.InvalidArgument, "Voting type is not valid."))
        };
    }

    private static VotingType MapToGrpcType(DbVotingType type)
    {
        return type switch
        {
            DbVotingType.SingleChoice => VotingType.SingleChoice,
            DbVotingType.MultipleChoice => VotingType.MultipleChoice,
            DbVotingType.Rating => VotingType.Rating,
            DbVotingType.OpenAnswer => VotingType.OpenAnswer,
            _ => throw new RpcException(new Status(StatusCode.InvalidArgument, "Voting type is not valid."))
        };
    }

    private static DbVotingStatus MapToDbStatus(VotingStatus status)
    {
        return status switch
        {
            VotingStatus.Draft          => DbVotingStatus.Draft,
            VotingStatus.Active         => DbVotingStatus.Active,
            VotingStatus.Paused         => DbVotingStatus.Paused,
            VotingStatus.Finished       => DbVotingStatus.Finished,
            VotingStatus.PendingApproval => DbVotingStatus.PendingApproval,
            VotingStatus.Unspecified    => throw new RpcException(
                new Status(StatusCode.InvalidArgument, "Voting status must be specified.")),
            _ => throw new RpcException(new Status(StatusCode.InvalidArgument, "Voting status is not valid."))
        };
    }

    private static VotingStatus MapToGrpcStatus(DbVotingStatus status)
    {
        return status switch
        {
            DbVotingStatus.Draft           => VotingStatus.Draft,
            DbVotingStatus.Active          => VotingStatus.Active,
            DbVotingStatus.Paused          => VotingStatus.Paused,
            DbVotingStatus.Finished        => VotingStatus.Finished,
            DbVotingStatus.PendingApproval => VotingStatus.PendingApproval,
            _ => throw new RpcException(new Status(StatusCode.InvalidArgument, "Voting status is not valid."))
        };
    }
    
    public static IEnumerable<VotingResponse> MapToResponseList(this IEnumerable<DbVoting> source)
    {
        return source.Adapt<IEnumerable<VotingResponse>>(Config);
    }
    
    public static IEnumerable<DbVoting> MapToEntityList(this IEnumerable<VotingResponse> source)
    {
        return source.Adapt<IEnumerable<DbVoting>>(Config);
    }
}