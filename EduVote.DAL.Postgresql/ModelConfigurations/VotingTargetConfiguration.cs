using EduVote.DAL.Postgresql.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduVote.DAL.Postgresql.ModelConfigurations;

public class VotingTargetConfiguration : IEntityTypeConfiguration<VotingTarget>
{
    private const int MaxLength = 128;
    
    public void Configure(EntityTypeBuilder<VotingTarget> builder)
    {
        builder.ToTable("VotingTargets");
        
        builder.HasKey(x => new { x.VotingId, x.EducationUnitId });
        
        // Voting
        builder
            .HasOne(x => x.Voting)
            .WithMany(x => x.VotingTargets)
            .HasForeignKey(x => x.VotingId);

        // EducationUnit
        builder
            .HasOne(x => x.EducationUnit)
            .WithMany(x => x.VotingTargets)
            .HasForeignKey(x => x.EducationUnitId);
    }
}