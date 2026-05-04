using EduVote.DAL.Postgresql.Context;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduVote.DAL.Postgresql.Services;

public class VotingRepository(EduVoteDbContext dbContext) : IVotingRepository
{
    public async Task<Voting> CreateAsync(Voting votingModel, CancellationToken cancellationToken = default)
    {
        dbContext.Votings.Add(votingModel);
        await dbContext.SaveChangesAsync(cancellationToken);

        return votingModel;
    }

    public async Task<Voting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Votings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Voting?> UpdateAsync(Voting votingModel, CancellationToken cancellationToken = default)
    {
        var existingVoting = await dbContext.Votings.FirstOrDefaultAsync(x => x.Id == votingModel.Id, cancellationToken);
        if (existingVoting is null)
        {
            return null;
        }

        existingVoting.Title = votingModel.Title;
        existingVoting.Description = votingModel.Description;
        existingVoting.Type = votingModel.Type;
        existingVoting.IsAnonymous = votingModel.IsAnonymous;
        existingVoting.AllowVoteChange = votingModel.AllowVoteChange;
        existingVoting.StartTime = votingModel.StartTime;
        existingVoting.EndTime = votingModel.EndTime;

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
