using MediatR;

namespace EduVote.Application.RolesCatalog.GetRoles;

public sealed record GetRolesQuery : IRequest<IReadOnlyList<RoleDetails>>;
