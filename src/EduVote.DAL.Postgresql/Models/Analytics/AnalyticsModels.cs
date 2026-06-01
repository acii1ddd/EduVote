namespace EduVote.DAL.Postgresql.Models.Analytics;

public record AnalyticsOverviewData(
    int TotalVotings,
    int ActiveVotings,
    int PendingApprovalVotings,
    int FinishedVotings,
    int DraftVotings,
    int PausedVotings,
    int TotalVotesCast,
    IReadOnlyList<StatusCountRow> StatusCounts,
    IReadOnlyList<TypeCountRow> TypeCounts,
    IReadOnlyList<TimeSeriesRow> VotingsCreatedSeries,
    IReadOnlyList<TimeSeriesRow> VotesCastSeries,
    IReadOnlyList<PendingApprovalRow> PendingApprovalQueue);

public record StatusCountRow(string Status, int Count);

public record TypeCountRow(string Type, int Count);

public record TimeSeriesRow(string Period, int Count);

public record PendingApprovalRow(Guid VotingId, string Title, Guid CreatedById, DateTime CreatedAt);

public record AnalyticsVotingListItem(
    Guid Id,
    string Title,
    string Type,
    string Status,
    DateTime StartTime,
    DateTime EndTime,
    DateTime CreatedAt,
    int TotalVotes,
    int EligibleCount,
    double TurnoutPercent,
    Guid CreatedById);

public record AnalyticsFilters(
    DateTime? DateFrom,
    DateTime? DateTo,
    Guid? EducationUnitId,
    string? Type = null,
    string? Status = null,
    Guid? CreatedById = null,
    int Page = 1,
    int PageSize = 20)
{
    public static AnalyticsFilters ForOverview(DateTime? dateFrom, DateTime? dateTo, Guid? educationUnitId) =>
        new(dateFrom, dateTo, educationUnitId);
}

public record UnitParticipationRow(
    Guid EducationUnitId,
    string EducationUnitName,
    int EligibleCount,
    int VotesCount);

public record VotingReportData(
    Guid VotingId,
    string Title,
    string Description,
    string Type,
    string Status,
    bool IsAnonymous,
    DateTime StartTime,
    DateTime EndTime,
    DateTime? CalculatedAt,
    int TotalVotes,
    int EligibleCount,
    double TurnoutPercent,
    string? ResultHash,
    string? TxHash,
    string? EtherscanUrl,
    string? ResultDataJson,
    int VoteHashesCount,
    IReadOnlyList<UnitParticipationRow> UnitBreakdown);
