using EduVote.Application.Votings.SeedDemoVotes;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.Tests.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.Tests.Votings;

public class SeedDemoVotesIntegrationTests(EduVoteApiFactory factory)
    : IClassFixture<EduVoteApiFactory>, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SeedDemoVotes_Should_Create_Votes_Without_Writing_VotingResult()
    {
        var votingId = await SeedActiveSingleChoiceVotingAsync("Принцесса университета demo");

        using var scope = factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(new SeedDemoVotesCommand("Принцесса", 25));

        Assert.Equal(votingId, result.VotingId);
        Assert.Equal(25, result.VoteCount);

        var voteCount = await factory.ExecuteDbContextAsync(async dbContext =>
            await dbContext.Votes.CountAsync(v => v.VotingId == votingId));

        Assert.Equal(25, voteCount);

        var hasVotingResult = await factory.ExecuteDbContextAsync(async dbContext =>
            await dbContext.VotingResults.AnyAsync(r => r.VotingId == votingId));

        Assert.False(hasVotingResult);
    }

    private async Task<Guid> SeedActiveSingleChoiceVotingAsync(string title)
    {
        var adminRoleId = Guid.NewGuid();
        var studentRoleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var votingId = Guid.NewGuid();
        var candidateA = Guid.NewGuid();
        var candidateB = Guid.NewGuid();

        await factory.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Roles.AddRange(
                new Role { Id = adminRoleId, Name = Roles.Administrator },
                new Role { Id = studentRoleId, Name = Roles.Student });
            dbContext.Users.Add(new User
            {
                Id = userId,
                Email = $"admin-{userId:N}@example.com",
                Name = "Administrator",
                PasswordHash = "hash",
                RoleId = adminRoleId
            });

            dbContext.Votings.Add(new Voting
            {
                Id = votingId,
                Title = title,
                Description = "Demo seed test",
                Type = DbVotingType.SingleChoice,
                IsAnonymous = false,
                AllowVoteChange = false,
                StartTime = DateTime.UtcNow.AddDays(-1),
                EndTime = DateTime.UtcNow.AddDays(7),
                Status = DbVotingStatus.Active,
                CreatedById = userId
            });

            dbContext.Candidates.AddRange(
                new Candidate
                {
                    Id = candidateA,
                    VotingId = votingId,
                    Name = "Анна",
                    Description = "A"
                },
                new Candidate
                {
                    Id = candidateB,
                    VotingId = votingId,
                    Name = "Борис",
                    Description = "B"
                });

            await dbContext.SaveChangesAsync();
        });

        return votingId;
    }
}
