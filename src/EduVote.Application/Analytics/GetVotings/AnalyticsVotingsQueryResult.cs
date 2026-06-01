using EduVote.DAL.Postgresql.Models.Analytics;

namespace EduVote.Application.Analytics.GetVotings;

public sealed record AnalyticsVotingsQueryResult(
    IReadOnlyList<AnalyticsVotingListItem> Items,
    int TotalCount,
    int Page,
    int PageSize);
