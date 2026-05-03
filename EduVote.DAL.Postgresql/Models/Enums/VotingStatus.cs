namespace EduVote.DAL.Postgresql.Models.Enums;

public enum VotingStatus
{
    Draft = 0, // created but not started
    Active = 1,
    Paused = 2,
    Finished = 3
}