using EduVote.DAL.Postgresql.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduVote.DAL.Postgresql.ModelConfigurations;

public class VoteConfiguration : IEntityTypeConfiguration<Vote>
{
    private const int MaxLength = 512;
    
    public void Configure(EntityTypeBuilder<Vote> builder)
    {
        builder.ToTable("Votes");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.VoteHash)
            .HasMaxLength(MaxLength)
            .IsRequired();
        
        builder.Property(x => x.CreatedAt).IsRequired();
        
        // Voting
        builder.HasOne(x => x.Voting)
            .WithMany(x => x.Votes)
            .HasForeignKey(x => x.VotingId);
        
        // User
        builder.HasOne(x => x.User)
            .WithMany(x => x.Votes)
            .HasForeignKey(x => x.UserId);
        
        // Candidate
        builder.HasOne(x => x.Candidate)
            .WithMany(x => x.Votes)
            .HasForeignKey(x => x.CandidateId);
        
        builder.HasIndex(x => new { x.UserId, x.VotingId, x.CandidateId })
            .IsUnique();
    }
}