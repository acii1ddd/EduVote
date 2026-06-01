using EduVote.DAL.Postgresql.Models.Analytics;

namespace EduVote.Application.Analytics.Services;

public interface IAnalyticsOverviewReportPdfGenerator
{
    byte[] Generate(AnalyticsOverviewData overview, AnalyticsFilters filters);
}
