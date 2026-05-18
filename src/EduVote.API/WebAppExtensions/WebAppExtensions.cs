using EduVote.API.Services.Grpc;
using EduVote.API.Services.Storage;
using EduVote.DAL.Postgresql;
using EduVote.DAL.Postgresql.Repositories;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
        
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors("AllowAll");

        // Must be registered before MapGrpcService to take routing priority over gRPC JSON transcoding
        app.MapPost("/api/candidates/{candidateId}/photo", async (
            string candidateId,
            IFormFile photo,
            ICandidateRepository candidateRepository,
            IFileStorageService fileStorageService,
            CancellationToken cancellationToken) =>
        {
            if (!Guid.TryParse(candidateId, out var id))
                return Results.BadRequest("Candidate id must be a valid GUID.");

            var candidate = await candidateRepository.GetByIdAsync(id, cancellationToken);
            if (candidate is null)
                return Results.NotFound($"Candidate '{candidateId}' not found.");

            if (photo.Length == 0)
                return Results.BadRequest("Photo file is empty.");

            var extension = Path.GetExtension(photo.FileName);
            var objectName = $"{candidate.Name}{extension}";

            await using var stream = photo.OpenReadStream();
            var photoUrl = await fileStorageService.UploadFileAsync(
                stream, photo.ContentType, objectName, candidate.Id, cancellationToken);

            candidate.PhotoObjectName = objectName;
            await candidateRepository.SaveChangesAsync(cancellationToken);

            return Results.Ok(new { photo_url = photoUrl });
        }).DisableAntiforgery();

        // gRPC services
        app.MapGrpcService<VotingService>();
        app.MapGrpcService<CandidateService>();
        app.MapGrpcService<VotingTargetService>();
        app.MapGrpcService<UserService>();
        app.MapGrpcService<AuthService>();
        app.MapGrpcService<UserEducationUnitsService>();
        app.MapGrpcService<RoleService>();

        app.UseFileServer();
    }
}
