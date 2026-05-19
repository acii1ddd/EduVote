using EduVote.API.Services.Analytics;
using EduVote.API.Services.Tools;
using EduVote.DAL.Postgresql.Models.Analytics;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using Google.Api;
using Google.Protobuf;

namespace EduVote.API.Services.Grpc;

public class AnalyticsService(
    IAnalyticsRepository analyticsRepository,
    IVotingReportPdfGenerator votingReportPdfGenerator,
    IOverviewReportPdfGenerator overviewReportPdfGenerator) 
    : API.Analytics.AnalyticsBase
{
    public override async Task<AnalyticsOverviewResponse> GetOverview(
        GetOverviewRequest request,
        ServerCallContext context)
    {
        //AdminGrpcAuthorization.EnsureAdministrator(context);

        var filters = AnalyticsFilterMapper.FromOverview(request);
        
        var data = await analyticsRepository
            .GetOverviewAsync(filters, context.CancellationToken);

        var response = new AnalyticsOverviewResponse
        {
            TotalVotings = data.TotalVotings,
            ActiveVotings = data.ActiveVotings,
            PendingApprovalVotings = data.PendingApprovalVotings,
            FinishedVotings = data.FinishedVotings,
            DraftVotings = data.DraftVotings,
            PausedVotings = data.PausedVotings,
            TotalVotesCast = data.TotalVotesCast
        };

        response.StatusCounts.AddRange(data.StatusCounts.Select(s => new StatusCount
        {
            Status = s.Status,
            Count = s.Count,
        }));

        response.TypeCounts.AddRange(data.TypeCounts.Select(t => new TypeCount
        {
            Type = t.Type,
            Count = t.Count,
        }));

        response.VotingsCreatedSeries.AddRange(data.VotingsCreatedSeries.Select(p => new TimeSeriesPoint
        {
            Period = p.Period,
            Count = p.Count,
        }));

        response.VotesCastSeries.AddRange(data.VotesCastSeries.Select(p => new TimeSeriesPoint
        {
            Period = p.Period,
            Count = p.Count,
        }));

        response.PendingApprovalQueue.AddRange(data.PendingApprovalQueue.Select(p =>
            new PendingApprovalRow
            {
                VotingId = p.VotingId.ToString(),
                Title = p.Title,
                CreatedById = p.CreatedById.ToString(),
                CreatedAt = Timestamp.FromDateTime(p.CreatedAt.ToUniversalTime()),
            }));

        return response;
    }

    public override async Task<AnalyticsVotingsResponse> GetVotings(
        GetAnalyticsVotingsRequest request,
        ServerCallContext context)
    {
        //AdminGrpcAuthorization.EnsureAdministrator(context);

        var filters = AnalyticsFilterMapper.FromVotings(request);
        var (items, totalCount) = await analyticsRepository.GetVotingsAsync(filters, context.CancellationToken);

        var response = new AnalyticsVotingsResponse
        {
            TotalCount = totalCount,
            Page = filters.Page,
            PageSize = filters.PageSize,
        };

        response.Items.AddRange(items.Select(MapVotingRow));
        return response;
    }

    public override async Task<HttpBody> DownloadVotingReport(
        DownloadVotingReportRequest request,
        ServerCallContext context)
    {
        //AdminGrpcAuthorization.EnsureAdministrator(context);

        var votingId = IdParser.ParseId(request.VotingId, "Voting");
        var reportData = await analyticsRepository.GetVotingReportDataAsync(
            votingId, context.CancellationToken);

        if (reportData is null)
        {
            throw new RpcException(new Status(
                StatusCode.FailedPrecondition,
                "PDF report is available only for finished votings with calculated results."));
        }

        var pdf = votingReportPdfGenerator.Generate(reportData);
        var fileName = BuildVotingFileName(reportData);

        SetContentDisposition(context, fileName);

        return new HttpBody
        {
            ContentType = "application/pdf",
            Data = ByteString.CopyFrom(pdf),
        };
    }

    public override async Task<HttpBody> DownloadOverviewReport(
        DownloadOverviewReportRequest request,
        ServerCallContext context)
    {
        //AdminGrpcAuthorization.EnsureAdministrator(context);

        var filters = AnalyticsFilterMapper.FromOverviewReport(request);
        var overview = await analyticsRepository
        .GetOverviewAsync(filters, context.CancellationToken);
        
        var pdf = overviewReportPdfGenerator.Generate(overview, filters);
        var fileName = BuildOverviewFileName(filters);

        SetContentDisposition(context, fileName);

        return new HttpBody
        {
            ContentType = "application/pdf",
            Data = ByteString.CopyFrom(pdf),
        };
    }

    private static AnalyticsVotingRow MapVotingRow(AnalyticsVotingListItem item) =>
        new()
        {
            Id = item.Id.ToString(),
            Title = item.Title,
            Type = item.Type,
            Status = item.Status,
            StartTime = Timestamp.FromDateTime(item.StartTime.ToUniversalTime()),
            EndTime = Timestamp.FromDateTime(item.EndTime.ToUniversalTime()),
            CreatedAt = Timestamp.FromDateTime(item.CreatedAt.ToUniversalTime()),
            TotalVotes = item.TotalVotes,
            EligibleCount = item.EligibleCount,
            TurnoutPercent = item.TurnoutPercent,
            CreatedById = item.CreatedById.ToString(),
        };

    private static string BuildVotingFileName(VotingReportData data)
    {
        var slug = Slugify(data.Title);
        return $"voting-{slug}-{data.VotingId:N}.pdf";
    }

    private static string BuildOverviewFileName(AnalyticsFilters filters)
    {
        var from = filters.DateFrom?.ToString("yyyyMMdd") ?? "all";
        var to = filters.DateTo?.ToString("yyyyMMdd") ?? "all";
        return $"eduvote-analytics-{from}-{to}.pdf";
    }

    private static string Slugify(string title)
    {
        var normalized = title.Trim().ToLowerInvariant();
        var sb = new StringBuilder();
        foreach (var ch in normalized)
        {
            if (char.IsLetterOrDigit(ch))
                sb.Append(ch);
            else if (ch is ' ' or '-' or '_')
                sb.Append('-');
        }

        var slug = sb.ToString().Trim('-');
        if (slug.Length > 40)
            slug = slug[..40].TrimEnd('-');
        return string.IsNullOrEmpty(slug) ? "report" : slug;
    }

    private static void SetContentDisposition(ServerCallContext context, string fileName)
    {
        // HTTP header values must be ASCII; non-ASCII names go in filename* (RFC 5987).
        var asciiFallback = ToAsciiFileName(fileName);
        var encoded = Uri.EscapeDataString(fileName);
        context.GetHttpContext().Response.Headers.ContentDisposition =
            $"attachment; filename=\"{asciiFallback}\"; filename*=UTF-8''{encoded}";
    }

    private static string ToAsciiFileName(string fileName)
    {
        var sb = new StringBuilder(fileName.Length);
        foreach (var ch in fileName)
        {
            if ((ch is >= 'a' and <= 'z') || (ch is >= 'A' and <= 'Z') || (ch is >= '0' and <= '9')
                || ch is '.' or '-')
                sb.Append(ch);
            else if (ch is ' ' or '_')
                sb.Append('-');
        }

        var result = sb.ToString().Trim('-');
        return string.IsNullOrEmpty(result) ? "report.pdf" : result;
    }
}
