using EduVote.DAL.Postgresql.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduVote.DAL.Postgresql.ModelConfigurations;

public class VotingConfiguration : IEntityTypeConfiguration<Voting>
{
    private const int MaxLength = 512;
    
    public void Configure(EntityTypeBuilder<Voting> builder)
    {
        builder.ToTable("Votings");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Title)
            .HasMaxLength(MaxLength)
            .IsRequired();
        
        builder.Property(x => x.Description)
            .HasMaxLength(MaxLength)
            .IsRequired();
        
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.IsAnonymous).IsRequired();
        builder.Property(x => x.AllowVoteChange).IsRequired();
        builder.Property(x => x.StartTime).IsRequired();
        builder.Property(x => x.EndTime).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.CreatedById).IsRequired();
        
        // Votes
        builder.HasMany(x => x.Votes)
            .WithOne(x => x.Voting)
            .HasForeignKey(x => x.VotingId);
        
        // Candidates
        builder.HasMany(x => x.Candidates)
            .WithOne(x => x.Voting)
            .HasForeignKey(x => x.VotingId);
        
        // VotingTargets
        builder.HasMany(x => x.VotingTargets)
            .WithOne(x => x.Voting)
            .HasForeignKey(x => x.VotingId);
        
        // VotingResult (one-to-one)
        builder.HasOne(x => x.VotingResult)
            .WithOne(x => x.Voting)
            .HasForeignKey<VotingResult>(x => x.VotingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}