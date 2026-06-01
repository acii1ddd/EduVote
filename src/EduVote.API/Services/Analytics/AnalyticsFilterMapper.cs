using EduVote.DAL.Postgresql.Models.Analytics;
using Google.Protobuf.WellKnownTypes;

namespace EduVote.API.Services.Analytics;

internal static class AnalyticsFilterMapper
{
    public static AnalyticsFilters FromOverview(GetOverviewRequest request) =>
        AnalyticsFilters.ForOverview(
            ToUtc(request.DateFrom),
            ToUtcInclusiveEnd(request.DateTo),
            ParseOptionalGuid(request.EducationUnitId));

    public static AnalyticsFilters FromVotings(GetAnalyticsVotingsRequest request) =>
        new(
            ToUtc(request.DateFrom),
            ToUtcInclusiveEnd(request.DateTo),
            ParseOptionalGuid(request.EducationUnitId),
            string.IsNullOrWhiteSpace(request.Type) ? null : request.Type,
            string.IsNullOrWhiteSpace(request.Status) ? null : request.Status,
            ParseOptionalGuid(request.CreatedById),
            request.Page > 0 ? request.Page : 1,
            request.PageSize > 0 ? request.PageSize : 20);

    public static AnalyticsFilters FromOverviewReport(DownloadOverviewReportRequest request) =>
        AnalyticsFilters.ForOverview(
            ToUtc(request.DateFrom),
            ToUtcInclusiveEnd(request.DateTo),
            ParseOptionalGuid(request.EducationUnitId));

    private static DateTime? ToUtc(Timestamp? ts) =>
        ts is null ? null : ts.ToDateTime().ToUniversalTime();

    /// <summary>date_to is inclusive for the selected calendar day.</summary>
    private static DateTime? ToUtcInclusiveEnd(Timestamp? ts)
    {
        if (ts is null)
            return null;

        var date = ts.ToDateTime().ToUniversalTime();
        return date.TimeOfDay == TimeSpan.Zero
            ? date.Date.AddDays(1).AddTicks(-1)
            : date;
    }

    private static Guid? ParseOptionalGuid(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : Guid.TryParse(value, out var id) ? id : null;
}
