using EduVote.Application.Analytics.Services;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.Analytics.DownloadOverviewReport;

public sealed class DownloadOverviewReportQueryHandler(
    IAnalyticsRepository analyticsRepository,
    IAnalyticsOverviewReportPdfGenerator overviewReportPdfGenerator)
    : IRequestHandler<DownloadOverviewReportQuery, PdfDownloadResult>
{
    public async Task<PdfDownloadResult> Handle(
        DownloadOverviewReportQuery request,
        CancellationToken cancellationToken)
    {
        var overview = await analyticsRepository
            .GetOverviewAsync(request.Filters, cancellationToken);

        var pdf = overviewReportPdfGenerator.Generate(overview, request.Filters);
        var fileName = AnalyticsReportFileNames.BuildOverviewFileName(request.Filters);

        return new PdfDownloadResult(pdf, fileName);
    }
}
