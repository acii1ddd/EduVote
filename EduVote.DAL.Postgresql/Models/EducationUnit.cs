using EduVote.DAL.Postgresql.Models.Abstractions;
using EduVote.DAL.Postgresql.Models.Enums;

namespace EduVote.DAL.Postgresql.Models;

/// <summary>
/// Target audience for a particular voting
/// </summary>
public class EducationUnit : IBaseEntity
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Name of educational unit (class 10A, College IT, Group 1-IS, etc.)  
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of educational unit (school, college, faculty, department, etc.)
    /// </summary>
    public EducationUnitType Type { get; set; }
    
    /// <summary>
    /// Parent for EducationUnit (like school for class) or null if it's school directly
    /// </summary>
    public Guid? ParentId { get; set; }
    
    public EducationUnit? Parent { get; set; }
    
    public ICollection<EducationUnit> Children { get; set; } = [];
    
    /// <summary>
    /// All restrictions for votings on which this education unit participate
    /// </summary>
    public ICollection<VotingTarget> VotingTargets { get; set; } = [];
}