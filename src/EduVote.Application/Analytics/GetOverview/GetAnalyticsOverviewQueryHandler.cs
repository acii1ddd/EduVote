using EduVote.DAL.Postgresql.Models.Analytics;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.Analytics.GetOverview;

public sealed class GetAnalyticsOverviewQueryHandler(IAnalyticsRepository analyticsRepository)
    : IRequestHandler<GetAnalyticsOverviewQuery, AnalyticsOverviewData>
{
    public Task<AnalyticsOverviewData> Handle(
        GetAnalyticsOverviewQuery request,
        CancellationToken cancellationToken) =>
        analyticsRepository.GetOverviewAsync(request.Filters, cancellationToken);
}
