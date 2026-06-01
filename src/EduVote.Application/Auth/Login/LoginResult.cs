namespace EduVote.Application.Auth.Login;

public sealed record LoginResult(
    Guid UserId,
    string Role,
    string AccessToken);
