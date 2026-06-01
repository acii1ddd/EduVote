namespace EduVote.Application.Common;

public sealed class ApplicationErrorException(
    ApplicationErrorType errorType,
    string message)
    : Exception(message)
{
    public ApplicationErrorType ErrorType { get; } = errorType;
}
