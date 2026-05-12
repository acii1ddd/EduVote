using EduVote.DAL.Postgresql.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduVote.DAL.Postgresql.ModelConfigurations;

public class CandidateConfiguration : IEntityTypeConfiguration<Candidate>
{
    private const int MaxLength = 512;
    
    public void Configure(EntityTypeBuilder<Candidate> builder)
    {
        builder.ToTable("Candidates");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .HasMaxLength(MaxLength)
            .IsRequired();
        
        builder.Property(x => x.Description)
            .HasMaxLength(MaxLength)
            .IsRequired();

        builder.Property(x => x.PhotoObjectName)
            .HasMaxLength(MaxLength);
        
        // Votings
        builder.HasOne(x => x.Voting)
            .WithMany(x => x.Candidates)
            .HasForeignKey(x => x.VotingId);
        
        // Vote
        builder.HasMany(x => x.Votes)
            .WithOne(x => x.Candidate)
            .HasForeignKey(x => x.CandidateId);
    }
}