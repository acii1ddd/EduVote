using EduVote.DAL.Postgresql.Context;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduVote.DAL.Postgresql.Services;

public class VotingRepository(EduVoteDbContext dbContext) : IVotingRepository
{
    public async Task<Voting> CreateAsync(Voting voting, CancellationToken cancellationToken = default)
    {
        dbContext.Votings.Add(voting);
        await dbContext.SaveChangesAsync(cancellationToken);

        return voting;
    }

    public async Task<Voting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Votings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Voting?> UpdateAsync(Voting voting, CancellationToken cancellationToken = default)
    {
        var existingVoting = await dbContext.Votings.FirstOrDefaultAsync(x => x.Id == voting.Id, cancellationToken);
        if (existingVoting is null)
        {
            return null;
        }

        existingVoting.Title = voting.Title;
        existingVoting.Description = voting.Description;
        existingVoting.Type = voting.Type;
        existingVoting.IsAnonymous = voting.IsAnonymous;
        existingVoting.AllowVoteChange = voting.AllowVoteChange;
        existingVoting.StartTime = voting.StartTime;
        existingVoting.EndTime = voting.EndTime;

        await dbContext.SaveChangesAsync(cancellationToken);

        return existingVoting;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existingVoting = await dbContext.Votings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (existingVoting is null)
        {
            return false;
        }

        dbContext.Votings.Remove(existingVoting);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<Voting?> UpdateStatusAsync(Guid id, VotingStatus status, CancellationToken cancellationToken = default)
    {
        var existingVoting = await dbContext.Votings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (existingVoting is null)
        {
            return null;
        }

        existingVoting.VotingStatus = status;
        await dbContext.SaveChangesAsync(cancellationToken);

        return existingVoting;
    }
}
