using System.Text;
using EduVote.DAL.Postgresql.Models.Analytics;

namespace EduVote.Application.Analytics;

internal static class AnalyticsReportFileNames
{
    public static string BuildVotingFileName(VotingReportData data)
    {
        var slug = Slugify(data.Title);
        return $"voting-{slug}-{data.VotingId:N}.pdf";
    }

    public static string BuildOverviewFileName(AnalyticsFilters filters)
    {
        var from = filters.DateFrom?.ToString("yyyyMMdd") ?? "all";
        var to = filters.DateTo?.ToString("yyyyMMdd") ?? "all";
        return $"eduvote-analytics-{from}-{to}.pdf";
    }

    private static string Slugify(string title)
    {
        var normalized = title.Trim().ToLowerInvariant();
        var sb = new StringBuilder();

        foreach (var ch in normalized)
        {
            if (char.IsLetterOrDigit(ch))
                sb.Append(ch);
            else if (ch is ' ' or '-' or '_')
                sb.Append('-');
        }

        var slug = sb.ToString().Trim('-');
        if (slug.Length > 40)
            slug = slug[..40].TrimEnd('-');

        return string.IsNullOrEmpty(slug) ? "report" : slug;
    }
}
