using EduVote.API.Auth;
using EduVote.API.Mappers;
using EduVote.API.Services.Analytics;
using EduVote.API.Validators;
using EduVote.Application.Analytics.DownloadOverviewReport;
using EduVote.Application.Analytics.DownloadVotingReport;
using EduVote.Application.Analytics.GetOverview;
using EduVote.Application.Analytics.GetVotings;
using EduVote.Application.Common;
using Google.Api;
using Google.Protobuf;
using MediatR;

namespace EduVote.API.Services;

public class AnalyticsService(ISender sender) : API.Analytics.AnalyticsBase
{
    public override async Task<AnalyticsOverviewResponse> GetOverview(
        GetOverviewRequest request,
        ServerCallContext context)
    {
        AdminAuthorization.EnsureAdministrator(context);

        try
        {
            var filters = AnalyticsFilterMapper.FromOverview(request);
            var data = await sender.Send(
                new GetAnalyticsOverviewQuery(filters),
                context.CancellationToken);

            return AnalyticsResponseMapper.MapOverview(data);
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }
    }

    public override async Task<AnalyticsVotingsResponse> GetVotings(
        GetAnalyticsVotingsRequest request,
        ServerCallContext context)
    {
        AdminAuthorization.EnsureAdministrator(context);

        try
        {
            var filters = AnalyticsFilterMapper.FromVotings(request);
            var result = await sender.Send(
                new GetAnalyticsVotingsQuery(filters),
                context.CancellationToken);

            return AnalyticsResponseMapper.MapVotings(result);
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }
    }

    public override async Task<HttpBody> DownloadVotingReport(
        DownloadVotingReportRequest request,
        ServerCallContext context)
    {
        AdminAuthorization.EnsureAdministrator(context);

        var votingId = IdParser.ParseId(request.VotingId, "Voting");

        try
        {
            var pdf = await sender.Send(
                new DownloadVotingReportQuery(votingId),
                context.CancellationToken);

            SetContentDisposition(context, pdf.FileName);

            return new HttpBody
            {
                ContentType = pdf.ContentType,
                Data = ByteString.CopyFrom(pdf.Content),
            };
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }
    }

    public override async Task<HttpBody> DownloadOverviewReport(
        DownloadOverviewReportRequest request,
        ServerCallContext context)
    {
        AdminAuthorization.EnsureAdministrator(context);

        try
        {
            var filters = AnalyticsFilterMapper.FromOverviewReport(request);
            var pdf = await sender.Send(
                new DownloadOverviewReportQuery(filters),
                context.CancellationToken);

            SetContentDisposition(context, pdf.FileName);

            return new HttpBody
            {
                ContentType = pdf.ContentType,
                Data = ByteString.CopyFrom(pdf.Content),
            };
        }
        catch (ApplicationErrorException ex)
        {
            throw new RpcException(new Status(MapStatusCode(ex.ErrorType), ex.Message));
        }
    }

    private static void SetContentDisposition(ServerCallContext context, string fileName)
    {
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

    private static StatusCode MapStatusCode(ApplicationErrorType errorType) =>
        errorType switch
        {
            ApplicationErrorType.InvalidArgument => StatusCode.InvalidArgument,
            ApplicationErrorType.NotFound => StatusCode.NotFound,
            ApplicationErrorType.PermissionDenied => StatusCode.PermissionDenied,
            ApplicationErrorType.FailedPrecondition => StatusCode.FailedPrecondition,
            ApplicationErrorType.AlreadyExists => StatusCode.AlreadyExists,
            ApplicationErrorType.Unauthenticated => StatusCode.Unauthenticated,
            ApplicationErrorType.Unavailable => StatusCode.Unavailable,
            _ => StatusCode.Unknown
        };
}
