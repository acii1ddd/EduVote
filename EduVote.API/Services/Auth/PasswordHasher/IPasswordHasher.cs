namespace EduVote.API.Services.Auth.PasswordHasher;

public interface IPasswordHasher
{
    public bool Verify(string password, string passwordHash);
    
    public string Hash(string password);
}