using EduVote.DAL.Postgresql.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduVote.DAL.Postgresql;

public class DatabaseInitializer
{
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly EduVoteDbContext _context;

    //private static readonly string _passwordHash = "$2a$11$0p4EJ6BqWtZUkaZCBr.f8eyKFMGmfw/GeaI7h5uW3TOUyQoQBVR6y";
    
    public DatabaseInitializer(
        EduVoteDbContext context, 
        ILogger<DatabaseInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Initializing database...");
            
            var migrations = await _context.Database.GetPendingMigrationsAsync();
            if (!migrations.Any())
            {
                _logger.LogInformation("All migrations are applied.");
                return;
            }
            
            _logger.LogInformation("Migration...");
            await _context.Database.MigrateAsync();
            
            //_logger.LogInformation("Добавление данных...");
            //await SeedDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error with applying migrations.");
            throw;
        }
    }

    // private async Task SeedDataAsync()
    // {
    //     var users = await AddUsers();
    //     var books = await AddBooks();
    //     var reviews = await AddReviews(users, books);
    //     await AddComments(users, reviews);
    // }
}