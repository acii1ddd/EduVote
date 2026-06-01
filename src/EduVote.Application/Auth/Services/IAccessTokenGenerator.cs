namespace EduVote.Application.Auth.Services;

public interface IAccessTokenGenerator
{
    string GenerateAccessToken(Guid userId, string roleName);
}
