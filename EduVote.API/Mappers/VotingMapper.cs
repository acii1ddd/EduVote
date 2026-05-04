using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mapster;
using DataVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DataVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.API.Mappers;

public static class VotingMapper
{
    private static readonly TypeAdapterConfig Config = new TypeAdapterConfig();

    static VotingMapper()
    {
        // Сопоставление между DAL Voting и gRPC VotingResponse
        Config.NewConfig<DAL.Postgresql.Models.Voting, VotingResponse>()
            .Map(dest => dest.Id, src => src.Id.ToString())
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Type, src => MapToGrpcType(src.Type))
            .Map(dest => dest.IsAnonymous, src => src.IsAnonymous)
            .Map(dest => dest.AllowVoteChange, src => src.AllowVoteChange)
            .Map(dest => dest.StartTime, src => Timestamp.FromDateTime(src.StartTime.ToUniversalTime()))
            .Map(dest => dest.EndTime, src => Timestamp.FromDateTime(src.EndTime.ToUniversalTime()))
            .Map(dest => dest.Status, src => MapToGrpcStatus(src.VotingStatus))
            .Map(dest => dest.CreatedAt, src => Timestamp.FromDateTime(src.CreatedAt.ToUniversalTime()));

        // Обратное сопоставление (если понадобится)
        Config.NewConfig<CreateVotingRequest, DAL.Postgresql.Models.Voting>()
            .Map(dest => dest.Id, src => Guid.NewGuid()) // Генерация Id на стороне API
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Type, src => MapToDalType(src.Type))
            .Map(dest => dest.IsAnonymous, src => src.IsAnonymous)
            .Map(dest => dest.AllowVoteChange, src => src.AllowVoteChange)
            .Map(dest => dest.StartTime, src => src.StartTime.ToDateTime().ToUniversalTime())
            .Map(dest => dest.EndTime, src => src.EndTime.ToDateTime().ToUniversalTime())
            .Map(dest => dest.VotingStatus, _ => VotingStatus.Draft); // Создается как Draft

        Config.NewConfig<UpdateVotingRequest, DAL.Postgresql.Models.Voting>()
            .Map(dest => dest.Id, src => Guid.Parse(src.Id))
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Type, src => MapToDalType(src.Type))
            .Map(dest => dest.IsAnonymous, src => src.IsAnonymous)
            .Map(dest => dest.AllowVoteChange, src => src.AllowVoteChange)
            .Map(dest => dest.StartTime, src => src.StartTime.ToDateTime().ToUniversalTime())
            .Map(dest => dest.EndTime, src => src.EndTime.ToDateTime().ToUniversalTime());
        // VotingStatus не меняется при update, не маппим
    }

    public static VotingResponse MapToResponse(this DAL.Postgresql.Models.Voting source)
    {
        return source.Adapt<VotingResponse>(Config);
    }

    public static DAL.Postgresql.Models.Voting MapToEntity(this CreateVotingRequest request)
    {
        return request.Adapt<DAL.Postgresql.Models.Voting>(Config);
    }

    public static DAL.Postgresql.Models.Voting MapToEntity(this UpdateVotingRequest request)
    {
        return request.Adapt<DAL.Postgresql.Models.Voting>(Config);
    }

    private static DataVotingType MapToDalType(VotingType type)
    {
        return type switch
        {
            VotingType.SingleChoice => DataVotingType.SingleChoice,
            VotingType.MultipleChoice => DataVotingType.MultipleChoice,
            VotingType.Rating => DataVotingType.Rating,
            VotingType.OpenAnswer => DataVotingType.OpenAnswer,
            _ => throw new RpcException(new Status(StatusCode.InvalidArgument, "Voting type is not valid."))
        };
    }

    private static VotingType MapToGrpcType(DataVotingType type)
    {
        return type switch
        {
            DataVotingType.SingleChoice => VotingType.SingleChoice,
            DataVotingType.MultipleChoice => VotingType.MultipleChoice,
            DataVotingType.Rating => VotingType.Rating,
            DataVotingType.OpenAnswer => VotingType.OpenAnswer,
            _ => throw new RpcException(new Status(StatusCode.InvalidArgument, "Voting type is not valid."))
        };
    }

    private static DataVotingStatus MapToDalStatus(VotingStatus status)
    {
        return status switch
        {
            VotingStatus.Draft => DataVotingStatus.Draft,
            VotingStatus.Active => DataVotingStatus.Active,
            VotingStatus.Paused => DataVotingStatus.Paused,
            VotingStatus.Finished => DataVotingStatus.Finished,
            _ => throw new RpcException(new Status(StatusCode.InvalidArgument, "Voting status is not valid."))
        };
    }

    private static VotingStatus MapToGrpcStatus(DataVotingStatus status)
    {
        return status switch
        {
            DataVotingStatus.Draft => VotingStatus.Draft,
            DataVotingStatus.Active => VotingStatus.Active,
            DataVotingStatus.Paused => VotingStatus.Paused,
            DataVotingStatus.Finished => VotingStatus.Finished,
            _ => throw new RpcException(new Status(StatusCode.InvalidArgument, "Voting status is not valid."))
        };
    }
}