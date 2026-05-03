using EduVote.DAL.Postgresql;

namespace EduVote.API.WebAppExtensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        
        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        
        await initializer.InitializeAsync();
    }
}
