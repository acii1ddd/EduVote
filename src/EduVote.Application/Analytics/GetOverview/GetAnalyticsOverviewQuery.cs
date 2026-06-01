using EduVote.DAL.Postgresql.Models.Analytics;
using MediatR;

namespace EduVote.Application.Analytics.GetOverview;

public sealed record GetAnalyticsOverviewQuery(AnalyticsFilters Filters)
    : IRequest<AnalyticsOverviewData>;
