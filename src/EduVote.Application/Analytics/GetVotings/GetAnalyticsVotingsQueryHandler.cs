using EduVote.DAL.Postgresql.Models.Analytics;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.Analytics.GetVotings;

public sealed class GetAnalyticsVotingsQueryHandler(IAnalyticsRepository analyticsRepository)
    : IRequestHandler<GetAnalyticsVotingsQuery, AnalyticsVotingsQueryResult>
{
    public async Task<AnalyticsVotingsQueryResult> Handle(
        GetAnalyticsVotingsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await analyticsRepository
            .GetVotingsAsync(request.Filters, cancellationToken);

        return new AnalyticsVotingsQueryResult(
            items,
            totalCount,
            request.Filters.Page,
            request.Filters.PageSize);
    }
}
