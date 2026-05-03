using EduVote.DAL.Postgresql.Models.Abstractions;

namespace EduVote.DAL.Postgresql.Models;

/// <summary>
/// Table to indicate the target audience of a particular voting
/// </summary>
public class VotingTarget : IBaseEntity
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Voting for which the target audience will be determined
    /// </summary>
    public Guid VotingId { get; set; }

    public Voting Voting { get; set; } = null!;

    public Guid EducationUnitId { get; set; }

    public EducationUnit EducationUnit { get; set; } = null!;
}