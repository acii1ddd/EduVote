namespace EduVote.DAL.Postgresql.Models;

/// <summary>
/// Table to indicate the target audience of a particular voting
/// (Voting 1 - N VotingTarget N - 1 EducationUnit)
/// </summary>
public class VotingTarget
{
    /// <summary>
    /// Voting for which the target audience will be determined
    /// </summary>
    public Guid VotingId { get; set; }

    public Voting Voting { get; set; } = null!;

    public Guid EducationUnitId { get; set; }

    public EducationUnit EducationUnit { get; set; } = null!;
}