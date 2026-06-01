using EduVote.Application.Common;
using EduVote.Application.Votings;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.Votings.GetVotingsForUser;

public sealed class GetVotingsForUserQueryHandler(
    IUserRepository userRepository,
    IEducationUnitRepository educationUnitRepository,
    IVotingRepository votingRepository)
    : IRequestHandler<GetVotingsForUserQuery, IReadOnlyList<VotingDetails>>
{
    public async Task<IReadOnlyList<VotingDetails>> Handle(
        GetVotingsForUserQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository
            .GetByIdWithEducationUnitsAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                $"User with id '{request.UserId}' was not found.");
        }

        var userUnitIds = user.UserEducationUnits
            .Select(ueu => ueu.EducationUnitId)
            .ToList();

        var allUnitIds = userUnitIds.Count > 0
            ? (await educationUnitRepository
                .GetAllParentIdsAsync(userUnitIds, cancellationToken))
                .ToList()
            : [];

        var votings = await votingRepository
            .GetVotingsForEducationUnitsAsync(allUnitIds, cancellationToken);

        return votings.Select(v => v.ToDetails()).ToList();
    }
}
