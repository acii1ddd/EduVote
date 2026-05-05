namespace EduVote.API.Validators;

public static class VotingValidator
{
    public static void ValidateDateRange(Timestamp? startTime, Timestamp? endTime)
    {
        if (startTime is null || endTime is null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "start_time and end_time are required."));
        }

        var startDate = startTime.ToDateTime();
        var endDate = endTime.ToDateTime();

        var diff = endDate - startDate;

        if (diff < TimeSpan.FromHours(1))
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "end_time must be at least 1 hour greater than start_time."));
        }
        
        if (startDate > endDate)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "start_time must be less than end_time."));
        }
    }
}
