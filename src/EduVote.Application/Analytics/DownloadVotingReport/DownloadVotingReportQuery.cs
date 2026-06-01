using MediatR;
using PdfDownloadResult = EduVote.Application.Analytics.PdfDownloadResult;

namespace EduVote.Application.Analytics.DownloadVotingReport;

public sealed record DownloadVotingReportQuery(Guid VotingId)
    : IRequest<PdfDownloadResult>;
