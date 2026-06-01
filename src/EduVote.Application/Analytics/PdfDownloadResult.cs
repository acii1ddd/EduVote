namespace EduVote.Application.Analytics;

public sealed record PdfDownloadResult(
    byte[] Content,
    string FileName,
    string ContentType = "application/pdf");
