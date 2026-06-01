using EduVote.API.Mappers;
using EduVote.API.Services.Tools;
using EduVote.Application.Common;
using EduVote.Application.EducationUnits.AssignUserToEducationUnit;
using EduVote.Application.EducationUnits.GetEducationUnits;
using EduVote.Application.EducationUnits.RemoveUserFromEducationUnit;
using MediatR;

namespace EduVote.API.Services.Grpc;

// [Authorize(Roles = Roles.Administrator)]
public class UserEducationUnitsService(ISender sender) : UserEducationUnits.UserEducationUnitsBase
{
    public override async Task<GetEducationUnitsResponse> GetEducationUnits(
        Empty request,
        ServerCallContext context)
    {
        var units = await sender.Send(
            new GetEducationUnitsQuery(),
            context.CancellationToken);

        var response = new GetEducationUnitsResponse();
        response.EducationUnits.AddRange(units.MapToResponseList());

        return response;
    }

    public override async Task<Empty> AssignUserToEducationUnit(
        AssignUserToEducationUnitRequest request,
        ServerCallContext context)
    {
        var userId = IdParser.ParseId(request.UserId, "User");
        var educationUnitId = IdParser.ParseId(request.EducationUnitId, "Education unit");

        try
        {
            await sender.Send(
                new AssignUserToEducationUnitCommand(userId, educationUnitId),
                context.CancellationToken);
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }

        return new Empty();
    }

    public override async Task<Empty> RemoveUserFromEducationUnit(
        RemoveUserFromEducationUnitRequest request,
        ServerCallContext context)
    {
        var userId = IdParser.ParseId(request.UserId, "User");
        var educationUnitId = IdParser.ParseId(request.EducationUnitId, "Education unit");

        await sender.Send(
            new RemoveUserFromEducationUnitCommand(userId, educationUnitId),
            context.CancellationToken);

        return new Empty();
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
}
