using EduVote.Application.Analytics;
using EduVote.Application.Analytics.Services;
using EduVote.Application.Common;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.Analytics.DownloadVotingReport;

public sealed class DownloadVotingReportQueryHandler(
    IAnalyticsRepository analyticsRepository,
    IAnalyticsVotingReportPdfGenerator votingReportPdfGenerator)
    : IRequestHandler<DownloadVotingReportQuery, PdfDownloadResult>
{
    public async Task<PdfDownloadResult> Handle(
        DownloadVotingReportQuery request,
        CancellationToken cancellationToken)
    {
        var reportData = await analyticsRepository
            .GetVotingReportDataAsync(request.VotingId, cancellationToken);

        if (reportData is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.FailedPrecondition,
                "PDF report is available only for finished votings with calculated results.");
        }

        var pdf = votingReportPdfGenerator.Generate(reportData);
        var fileName = AnalyticsReportFileNames.BuildVotingFileName(reportData);

        return new PdfDownloadResult(pdf, fileName);
    }
}
