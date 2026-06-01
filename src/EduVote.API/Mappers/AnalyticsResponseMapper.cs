using EduVote.Application.Analytics.GetVotings;
using EduVote.DAL.Postgresql.Models.Analytics;

namespace EduVote.API.Mappers;

internal static class AnalyticsResponseMapper
{
    public static AnalyticsOverviewResponse MapOverview(AnalyticsOverviewData data)
    {
        var response = new AnalyticsOverviewResponse
        {
            TotalVotings = data.TotalVotings,
            ActiveVotings = data.ActiveVotings,
            PendingApprovalVotings = data.PendingApprovalVotings,
            FinishedVotings = data.FinishedVotings,
            DraftVotings = data.DraftVotings,
            PausedVotings = data.PausedVotings,
            TotalVotesCast = data.TotalVotesCast
        };

        response.StatusCounts.AddRange(data.StatusCounts.Select(s => new StatusCount
        {
            Status = s.Status,
            Count = s.Count,
        }));

        response.TypeCounts.AddRange(data.TypeCounts.Select(t => new TypeCount
        {
            Type = t.Type,
            Count = t.Count,
        }));

        response.VotingsCreatedSeries.AddRange(data.VotingsCreatedSeries.Select(p => new TimeSeriesPoint
        {
            Period = p.Period,
            Count = p.Count,
        }));

        response.VotesCastSeries.AddRange(data.VotesCastSeries.Select(p => new TimeSeriesPoint
        {
            Period = p.Period,
            Count = p.Count,
        }));

        response.PendingApprovalQueue.AddRange(data.PendingApprovalQueue.Select(p =>
            new PendingApprovalRow
            {
                VotingId = p.VotingId.ToString(),
                Title = p.Title,
                CreatedById = p.CreatedById.ToString(),
                CreatedAt = Timestamp.FromDateTime(p.CreatedAt.ToUniversalTime()),
            }));

        return response;
    }

    public static AnalyticsVotingsResponse MapVotings(AnalyticsVotingsQueryResult result)
    {
        var response = new AnalyticsVotingsResponse
        {
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
        };

        response.Items.AddRange(result.Items.Select(MapVotingRow));
        return response;
    }

    private static AnalyticsVotingRow MapVotingRow(AnalyticsVotingListItem item) =>
        new()
        {
            Id = item.Id.ToString(),
            Title = item.Title,
            Type = item.Type,
            Status = item.Status,
            StartTime = Timestamp.FromDateTime(item.StartTime.ToUniversalTime()),
            EndTime = Timestamp.FromDateTime(item.EndTime.ToUniversalTime()),
            CreatedAt = Timestamp.FromDateTime(item.CreatedAt.ToUniversalTime()),
            TotalVotes = item.TotalVotes,
            EligibleCount = item.EligibleCount,
            TurnoutPercent = item.TurnoutPercent,
            CreatedById = item.CreatedById.ToString(),
        };
}
