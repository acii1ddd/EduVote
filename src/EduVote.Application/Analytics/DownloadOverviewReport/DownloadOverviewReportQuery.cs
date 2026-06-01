using EduVote.DAL.Postgresql.Models.Analytics;
using MediatR;
using PdfDownloadResult = EduVote.Application.Analytics.PdfDownloadResult;

namespace EduVote.Application.Analytics.DownloadOverviewReport;

public sealed record DownloadOverviewReportQuery(AnalyticsFilters Filters) 
    : IRequest<PdfDownloadResult>;
