using EduVote.DAL.Postgresql.Models.Analytics;

namespace EduVote.DAL.Postgresql.Repositories.Interfaces;

public interface IAnalyticsRepository
{
    Task<AnalyticsOverviewData> GetOverviewAsync(
        AnalyticsFilters filters,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<AnalyticsVotingListItem> Items, int TotalCount)> GetVotingsAsync(
        AnalyticsFilters filters,
        CancellationToken cancellationToken = default);

    Task<VotingReportData?> GetVotingReportDataAsync(
        Guid votingId,
        CancellationToken cancellationToken = default);

    Task<int> CountEligibleUsersAsync(
        Guid votingId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UnitParticipationRow>> GetUnitParticipationAsync(
        Guid votingId,
        bool includeVoteCounts,
        CancellationToken cancellationToken = default);
}
