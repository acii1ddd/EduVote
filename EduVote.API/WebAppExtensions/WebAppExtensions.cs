using EduVote.API.Services.Grpc;
using EduVote.DAL.Postgresql;

namespace EduVote.API.WebAppExtensions;

public static class WebAppExtensions
{
    private static async Task ApplyMigrationsAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        
        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        
        await initializer.InitializeAsync();
    }
    
    public static async Task MapServicesAsync(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            Console.WriteLine("Swagger with gRPC transcoding is available on: https://localhost:5959/swagger/index.html");
    
            app.MapGrpcReflectionService()
                .AllowAnonymous();
    
            await app.ApplyMigrationsAsync();
        }
        
        // Configure the HTTP request pipeline.
        app.MapGrpcService<VotingService>();
        app.MapGrpcService<CandidateService>();
        app.MapGrpcService<VotingTargetService>();
        app.MapGrpcService<UserService>();
        app.MapGrpcService<AuthService>();
    }
}
