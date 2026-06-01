using EduVote.Application.Common;

namespace EduVote.Application.Votings.CreateVoting;

public static class CreateVotingValidator
{
    public static void ValidateDateRange(DateTime startTime, DateTime endTime)
    {
        var diff = endTime - startTime;

        if (diff < TimeSpan.FromHours(1))
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.InvalidArgument,
                "end_time must be at least 1 hour greater than start_time.");
        }

        if (startTime > endTime)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.InvalidArgument,
                "start_time must be less than end_time.");
        }
    }
}
