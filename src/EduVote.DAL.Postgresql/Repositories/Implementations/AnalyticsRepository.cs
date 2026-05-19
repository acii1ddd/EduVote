using EduVote.DAL.Postgresql.Context;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Analytics;
using EduVote.DAL.Postgresql.Models.Enums;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EduVote.DAL.Postgresql.Repositories.Implementations;

public class AnalyticsRepository(
    EduVoteDbContext dbContext,
    IEducationUnitRepository educationUnitRepository)
    : IAnalyticsRepository
{
    public async Task<AnalyticsOverviewData> GetOverviewAsync(
        AnalyticsFilters filters,
        CancellationToken cancellationToken = default)
    {
        var votingsQuery = await ApplyVotingFiltersAsync(dbContext.Votings.AsNoTracking(), filters, cancellationToken);

        var votings = await votingsQuery
            .Select(v => new { v.Id, v.Status, v.Type, v.CreatedAt, v.Title, v.CreatedById })
            .ToListAsync(cancellationToken);

        var votingIds = votings.Select(v => v.Id).ToList();

        var votesQuery = dbContext.Votes.AsNoTracking();
        if (filters.DateFrom.HasValue)
            votesQuery = votesQuery.Where(v => v.CreatedAt >= filters.DateFrom.Value);
        if (filters.DateTo.HasValue)
            votesQuery = votesQuery.Where(v => v.CreatedAt <= filters.DateTo.Value);

        votesQuery = votingIds.Count > 0
            ? votesQuery.Where(v => votingIds.Contains(v.VotingId))
            : votesQuery.Where(_ => false);

        var totalVotesCast = await votesQuery.CountAsync(cancellationToken);

        var statusCounts = votings
            .GroupBy(v => v.Status.ToString())
            .Select(g => new StatusCountRow(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

        var typeCounts = votings
            .GroupBy(v => v.Type.ToString())
            .Select(g => new TypeCountRow(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

        var votingsCreatedSeries = votings
            .GroupBy(v => ToWeekPeriod(v.CreatedAt))
            .Select(g => new TimeSeriesRow(g.Key, g.Count()))
            .OrderBy(x => x.Period)
            .ToList();

        var voteDates = await votesQuery
            .Select(v => v.CreatedAt)
            .ToListAsync(cancellationToken);

        var votesCastSeries = voteDates
            .GroupBy(d => ToWeekPeriod(d))
            .Select(g => new TimeSeriesRow(g.Key, g.Count()))
            .OrderBy(x => x.Period)
            .ToList();

        var pendingQueue = votings
            .Where(v => v.Status == VotingStatus.PendingApproval)
            .OrderByDescending(v => v.CreatedAt)
            .Take(10)
            .Select(v => new PendingApprovalRow(v.Id, v.Title, v.CreatedById, v.CreatedAt))
            .ToList();

        return new AnalyticsOverviewData(
            TotalVotings: votings.Count,
            ActiveVotings: votings.Count(v => v.Status == VotingStatus.Active),
            PendingApprovalVotings: votings.Count(v => v.Status == VotingStatus.PendingApproval),
            FinishedVotings: votings.Count(v => v.Status == VotingStatus.Finished),
            DraftVotings: votings.Count(v => v.Status == VotingStatus.Draft),
            PausedVotings: votings.Count(v => v.Status == VotingStatus.Paused),
            TotalVotesCast: totalVotesCast,
            StatusCounts: statusCounts,
            TypeCounts: typeCounts,
            VotingsCreatedSeries: votingsCreatedSeries,
            VotesCastSeries: votesCastSeries,
            PendingApprovalQueue: pendingQueue);
    }

    public async Task<(IReadOnlyList<AnalyticsVotingListItem> Items, int TotalCount)> GetVotingsAsync(
        AnalyticsFilters filters,
        CancellationToken cancellationToken = default)
    {
        var query = await ApplyVotingFiltersAsync(dbContext.Votings.AsNoTracking(), filters, cancellationToken);
        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(filters.Page, 1);
        var pageSize = Math.Clamp(filters.PageSize, 1, 100);

        var votings = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new
            {
                v.Id,
                v.Title,
                v.Type,
                v.Status,
                v.StartTime,
                v.EndTime,
                v.CreatedAt,
                v.CreatedById,
                TotalVotes = v.VotingResult != null
                    ? v.VotingResult.TotalVotes
                    : v.Votes.Count,
            })
            .ToListAsync(cancellationToken);

        var eligibleByVoting = await GetEligibleCountsAsync(
            votings.Select(v => v.Id),
            cancellationToken);

        var items = votings.Select(v =>
        {
            var eligible = eligibleByVoting.GetValueOrDefault(v.Id, 0);
            var turnout = eligible > 0
                ? Math.Round(v.TotalVotes * 100.0 / eligible, 2)
                : 0;

            return new AnalyticsVotingListItem(
                v.Id,
                v.Title,
                v.Type.ToString(),
                v.Status.ToString(),
                v.StartTime,
                v.EndTime,
                v.CreatedAt,
                v.TotalVotes,
                eligible,
                turnout,
                v.CreatedById);
        }).ToList();

        return (items, totalCount);
    }

    public async Task<VotingReportData?> GetVotingReportDataAsync(
        Guid votingId,
        CancellationToken cancellationToken = default)
    {
        var voting = await dbContext.Votings
            .AsNoTracking()
            .Include(v => v.VotingResult)
            .FirstOrDefaultAsync(v => v.Id == votingId, cancellationToken);

        if (voting is null)
            return null;

        if (voting.Status != VotingStatus.Finished || voting.VotingResult is null)
            return null;

        var totalVotes = voting.VotingResult.TotalVotes;
        var eligible = await CountEligibleUsersAsync(votingId, cancellationToken);
        var turnout = eligible > 0 ? Math.Round(totalVotes * 100.0 / eligible, 2) : 0;

        var includeVoteCounts = !voting.IsAnonymous;
        var unitBreakdown = await GetUnitParticipationAsync(
            votingId, includeVoteCounts, cancellationToken);

        var voteHashesCount = await dbContext.Votes
            .AsNoTracking()
            .CountAsync(v => v.VotingId == votingId, cancellationToken);

        string? txHash = null;
        string? etherscanUrl = null;
        var blockchain = await dbContext.BlockchainRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(
                b => b.VotingResultId == voting.VotingResult.Id,
                cancellationToken);

        if (blockchain is not null)
        {
            txHash = blockchain.TransactionHash;
            etherscanUrl = $"https://sepolia.etherscan.io/tx/{blockchain.TransactionHash}";
        }

        return new VotingReportData(
            voting.Id,
            voting.Title,
            voting.Description,
            voting.Type.ToString(),
            voting.Status.ToString(),
            voting.IsAnonymous,
            voting.StartTime,
            voting.EndTime,
            voting.VotingResult.CalculatedAt,
            totalVotes,
            eligible,
            turnout,
            voting.VotingResult.ResultHash,
            txHash,
            etherscanUrl,
            voting.VotingResult.ResultData,
            voteHashesCount,
            unitBreakdown);
    }

    public async Task<int> CountEligibleUsersAsync(
        Guid votingId,
        CancellationToken cancellationToken = default)
    {
        var counts = await GetEligibleCountsAsync([votingId], cancellationToken);
        return counts.GetValueOrDefault(votingId, 0);
    }

    public async Task<IReadOnlyList<UnitParticipationRow>> GetUnitParticipationAsync(
        Guid votingId,
        bool includeVoteCounts,
        CancellationToken cancellationToken = default)
    {
        var voting = await dbContext.Votings
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == votingId, cancellationToken);

        if (voting is null)
            return [];

        var targets = await dbContext.VotingTargets
            .AsNoTracking()
            .Where(t => t.VotingId == votingId)
            .Include(t => t.EducationUnit)
            .ToListAsync(cancellationToken);

        if (targets.Count == 0)
            return [];

        var rows = new List<UnitParticipationRow>();

        foreach (var target in targets)
        {
            var scopeIds = (await educationUnitRepository
                .GetAllDescendantIdsAsync([target.EducationUnitId], cancellationToken))
                .ToHashSet();

            var eligible = await dbContext.UserEducationUnits
                .AsNoTracking()
                .Where(ueu => scopeIds.Contains(ueu.EducationUnitId))
                .Select(ueu => ueu.UserId)
                .Distinct()
                .CountAsync(cancellationToken);

            var votesCount = 0;
            if (includeVoteCounts)
            {
                votesCount = await dbContext.Votes
                    .AsNoTracking()
                    .Where(v => v.VotingId == votingId &&
                                dbContext.UserEducationUnits.Any(ueu =>
                                    ueu.UserId == v.UserId &&
                                    scopeIds.Contains(ueu.EducationUnitId)))
                    .CountAsync(cancellationToken);
            }

            rows.Add(new UnitParticipationRow(
                target.EducationUnitId,
                target.EducationUnit.Name,
                eligible,
                votesCount));
        }

        return rows;
    }

    private async Task<Dictionary<Guid, int>> GetEligibleCountsAsync(
        IEnumerable<Guid> votingIds,
        CancellationToken cancellationToken)
    {
        var ids = votingIds.Distinct().ToList();
        if (ids.Count == 0)
            return [];

        var targets = await dbContext.VotingTargets
            .AsNoTracking()
            .Where(t => ids.Contains(t.VotingId))
            .ToListAsync(cancellationToken);

        var votingsWithTargets = targets.Select(t => t.VotingId).ToHashSet();
        var result = new Dictionary<Guid, int>();

        var publicIds = ids.Where(id => !votingsWithTargets.Contains(id)).ToList();
        if (publicIds.Count > 0)
        {
            var allUsers = await dbContext.Users.AsNoTracking().CountAsync(cancellationToken);
            foreach (var id in publicIds)
                result[id] = allUsers;
        }

        foreach (var group in targets.GroupBy(t => t.VotingId))
        {
            var targetUnitIds = group.Select(t => t.EducationUnitId).ToList();
            var expandedIds = (await educationUnitRepository
                .GetAllDescendantIdsAsync(targetUnitIds, cancellationToken))
                .ToHashSet();

            var eligible = await dbContext.UserEducationUnits
                .AsNoTracking()
                .Where(ueu => expandedIds.Contains(ueu.EducationUnitId))
                .Select(ueu => ueu.UserId)
                .Distinct()
                .CountAsync(cancellationToken);

            result[group.Key] = eligible;
        }

        return result;
    }

    private async Task<IQueryable<Voting>> ApplyVotingFiltersAsync(
        IQueryable<Voting> query,
        AnalyticsFilters filters,
        CancellationToken cancellationToken)
    {
        if (filters.DateFrom.HasValue)
            query = query.Where(v => v.CreatedAt >= filters.DateFrom.Value);
        if (filters.DateTo.HasValue)
            query = query.Where(v => v.CreatedAt <= filters.DateTo.Value);
        if (filters.CreatedById.HasValue)
            query = query.Where(v => v.CreatedById == filters.CreatedById.Value);

        if (!string.IsNullOrWhiteSpace(filters.Status) &&
            Enum.TryParse<VotingStatus>(filters.Status, true, out var status))
        {
            query = query.Where(v => v.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(filters.Type) &&
            Enum.TryParse<VotingType>(filters.Type, true, out var type))
        {
            query = query.Where(v => v.Type == type);
        }

        if (filters.EducationUnitId.HasValue)
        {
            var scopeIds = (await educationUnitRepository
                .GetAllDescendantIdsAsync([filters.EducationUnitId.Value], cancellationToken))
                .ToHashSet();

            query = query.Where(v =>
                v.VotingTargets.Any(t => scopeIds.Contains(t.EducationUnitId)));
        }

        return query;
    }

    private static string ToWeekPeriod(DateTime date) =>
        $"{date.Year}-W{System.Globalization.ISOWeek.GetWeekOfYear(date.Date):D2}";
}
