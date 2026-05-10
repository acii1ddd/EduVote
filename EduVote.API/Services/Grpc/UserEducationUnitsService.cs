using EduVote.API.Mappers;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.DAL.Postgresql.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace EduVote.API.Services.Grpc;

// [Authorize(Roles = Roles.Administrator)]
public class UserEducationUnitsService(
    IUserEducationUnitRepository userEducationUnitRepository,
    IEducationUnitRepository educationUnitRepository)
    : UserEducationUnits.UserEducationUnitsBase
{
    public override async Task<GetEducationUnitsResponse>
        GetEducationUnits(
            Empty request,
            ServerCallContext context)
    {
        var educationUnits = await educationUnitRepository
            .GetAllAsync(context.CancellationToken);

        var response = new GetEducationUnitsResponse();

        response.EducationUnits.AddRange(
            educationUnits.MapToResponseList());

        return response;
    }

    public override async Task<Empty>
        AssignUserToEducationUnit(
            AssignUserToEducationUnitRequest request,
            ServerCallContext context)
    {
        var relation = new UserEducationUnit
        {
            UserId = Guid.Parse(request.UserId),
            EducationUnitId = Guid.Parse(request.EducationUnitId)
        };

        await userEducationUnitRepository.AddAsync(
            relation,
            context.CancellationToken
        );

        return new Empty();
    }

    public override async Task<Empty>
        RemoveUserFromEducationUnit(
            RemoveUserFromEducationUnitRequest request,
            ServerCallContext context)
    {
        await userEducationUnitRepository.RemoveAsync(
            Guid.Parse(request.UserId),
            Guid.Parse(request.EducationUnitId),
            context.CancellationToken
        );

        return new Empty();
    }
}