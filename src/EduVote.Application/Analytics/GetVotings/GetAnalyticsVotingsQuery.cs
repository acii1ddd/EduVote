using EduVote.DAL.Postgresql.Models.Analytics;
using MediatR;

namespace EduVote.Application.Analytics.GetVotings;

public sealed record GetAnalyticsVotingsQuery(AnalyticsFilters Filters)
    : IRequest<AnalyticsVotingsQueryResult>;
