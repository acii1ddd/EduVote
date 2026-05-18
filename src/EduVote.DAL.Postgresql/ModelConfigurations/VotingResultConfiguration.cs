using EduVote.DAL.Postgresql.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduVote.DAL.Postgresql.ModelConfigurations;

public class VotingResultConfiguration : IEntityTypeConfiguration<VotingResult>
{
    public void Configure(EntityTypeBuilder<VotingResult> builder)
    {
        builder.ToTable("VotingResults");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.VotingId)
            .IsRequired();
        
        builder.Property(x => x.ResultData)
            .HasColumnType("jsonb")
            .IsRequired();
        
        builder.Property(x => x.ResultHash)
            .HasMaxLength(256)
            .IsRequired();
        
        builder.Property(x => x.CalculatedAt)
            .IsRequired();
        
        builder.Property(x => x.TotalVotes)
            .IsRequired();
        
        // One-to-one relationship with Voting
        builder.HasOne(x => x.Voting)
            .WithOne(x => x.VotingResult)
            .HasForeignKey<VotingResult>(x => x.VotingId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // One-to-many relationship with BlockchainRecords
        builder.HasMany<BlockchainRecord>()
            .WithOne(x => x.VotingResult)
            .HasForeignKey(x => x.VotingResultId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Index on VotingId for quick lookup
        builder.HasIndex(x => x.VotingId)
            .IsUnique();
        
        // Index on CalculatedAt for sorting
        builder.HasIndex(x => x.CalculatedAt);
    }
}
