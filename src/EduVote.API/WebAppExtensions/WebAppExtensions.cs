using EduVote.API.Services;
using EduVote.Application.Candidates.UploadCandidatePhoto;
using EduVote.Application.Common;
using EduVote.DAL.Postgresql;
using EduVote.DAL.Postgresql.Repositories;
using MediatR;
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
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            if (!Guid.TryParse(candidateId, out var id))
                return Results.BadRequest("Candidate id must be a valid GUID.");

            if (photo.Length == 0)
                return Results.BadRequest("Photo file is empty.");

            try
            {
                await using var stream = photo.OpenReadStream();
                var photoUrl = await sender.Send(
                    new UploadCandidatePhotoCommand(
                        id,
                        stream,
                        photo.ContentType,
                        photo.FileName),
                    cancellationToken);

                return Results.Ok(new { photo_url = photoUrl });
            }
            catch (ApplicationErrorException ex)
            {
                return MapApplicationError(ex);
            }
        }).DisableAntiforgery();

        // gRPC services
        app.MapGrpcService<VotingService>();
        app.MapGrpcService<CandidateService>();
        app.MapGrpcService<VotingTargetService>();
        app.MapGrpcService<UserService>();
        app.MapGrpcService<AuthService>();
        app.MapGrpcService<UserEducationUnitsService>();
        app.MapGrpcService<RoleService>();
        app.MapGrpcService<AnalyticsService>();

        app.UseFileServer();
    }

    private static IResult MapApplicationError(ApplicationErrorException ex) =>
        ex.ErrorType switch
        {
            ApplicationErrorType.NotFound => Results.NotFound(ex.Message),
            ApplicationErrorType.InvalidArgument => Results.BadRequest(ex.Message),
            ApplicationErrorType.AlreadyExists => Results.Conflict(ex.Message),
            ApplicationErrorType.Unauthenticated => Results.Unauthorized(),
            ApplicationErrorType.PermissionDenied => Results.Forbid(),
            ApplicationErrorType.FailedPrecondition => Results.BadRequest(ex.Message),
            ApplicationErrorType.Unavailable => Results.StatusCode(StatusCodes.Status503ServiceUnavailable),
            _ => Results.Problem(ex.Message)
        };
}
