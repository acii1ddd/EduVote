using EduVote.DAL.Postgresql.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduVote.DAL.Postgresql.ModelConfigurations;

public class VoteConfiguration : IEntityTypeConfiguration<Vote>
{
    private const int VoteHashMaxLength = 512;
    private const int VoteSaltMaxLength = 36;
    private const int VotesMaxLength = 2000;
    
    public void Configure(EntityTypeBuilder<Vote> builder)
    {
        builder.ToTable("Votes");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.VoteSalt)
            .HasMaxLength(VoteSaltMaxLength)
            .IsRequired();

        builder.Property(x => x.VoteHash)
            .HasMaxLength(VoteHashMaxLength)
            .IsRequired();
        
        builder.Property(x => x.CreatedAt).IsRequired();
        
        // Single Choice - CandidateId is now nullable
        builder.Property(x => x.CandidateId).IsRequired(false);
        
        // Multiple Choice
        builder.Property(x => x.SelectedCandidateIds)
            .HasColumnType("jsonb")
            .HasMaxLength(VotesMaxLength)
            .IsRequired(false);
        
        // Rating
        builder.Property(x => x.RatingAnswers)
            .HasColumnType("jsonb")
            .HasMaxLength(VotesMaxLength)
            .IsRequired(false);
        
        // Open Answer
        builder.Property(x => x.TextAnswer)
            .HasMaxLength(VotesMaxLength)
            .IsRequired(false);
        
        // Voting
        builder.HasOne(x => x.Voting)
            .WithMany(x => x.Votes)
            .HasForeignKey(x => x.VotingId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // User
        builder.HasOne(x => x.User)
            .WithMany(x => x.Votes)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Candidate - optional for open votings
        builder.HasOne(x => x.Candidate)
            .WithMany(x => x.Votes)
            .HasForeignKey(x => x.CandidateId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Index for unique vote per user per voting (works for Single Choice)
        builder.HasIndex(x => new { x.UserId, x.VotingId })
            .IsUnique();
    }
}