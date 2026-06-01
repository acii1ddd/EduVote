using EduVote.DAL.Postgresql.Models.Analytics;

namespace EduVote.API.Services.Analytics;

public interface IVotingReportPdfGenerator
{
    byte[] Generate(VotingReportData data);
}

public interface IOverviewReportPdfGenerator
{
    byte[] Generate(AnalyticsOverviewData overview, AnalyticsFilters filters);
}
