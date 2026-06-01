using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.RolesCatalog.GetRoles;

public sealed class GetRolesQueryHandler(IRoleRepository roleRepository)
    : IRequestHandler<GetRolesQuery, IReadOnlyList<RoleDetails>>
{
    public async Task<IReadOnlyList<RoleDetails>> Handle(
        GetRolesQuery request,
        CancellationToken cancellationToken)
    {
        var roles = await roleRepository.GetAllAsync();

        return roles.Select(r => r.ToDetails()).ToList();
    }
}
