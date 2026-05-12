namespace EduVote.DAL.Postgresql.Models.Enums;

public enum VotingStatus
{
    Draft = 0,
    Active = 1,
    Paused = 2,
    Finished = 3,
    PendingApproval = 4, // created by Teacher, waiting for Administrator approval
}