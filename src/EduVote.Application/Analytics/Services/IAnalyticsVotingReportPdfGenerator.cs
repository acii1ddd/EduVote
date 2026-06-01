using EduVote.DAL.Postgresql.Models.Analytics;

namespace EduVote.Application.Analytics.Services;

public interface IAnalyticsVotingReportPdfGenerator
{
    byte[] Generate(VotingReportData data);
}
