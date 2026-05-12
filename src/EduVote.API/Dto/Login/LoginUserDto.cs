namespace EduVote.API.Dto.Login;

public record LoginUserDto(Guid Id, string Role, string AccessToken);