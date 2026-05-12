using EduVote.DAL.Postgresql.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduVote.DAL.Postgresql.ModelConfigurations;

public class BlockchainRecordConfiguration : IEntityTypeConfiguration<BlockchainRecord>
{
    public void Configure(EntityTypeBuilder<BlockchainRecord> builder)
    {
        builder.ToTable("BlockchainRecords");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.VotingResultId)
            .IsRequired();
        
        builder.Property(x => x.VotingId)
            .IsRequired();
        
        builder.Property(x => x.TransactionHash)
            .HasMaxLength(256)
            .IsRequired();
        
        builder.Property(x => x.VotesHash)
            .HasMaxLength(256)
            .IsRequired();
        
        builder.Property(x => x.BlockNumber)
            .IsRequired();
        
        builder.Property(x => x.Network)
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(x => x.SmartContractAddress)
            .HasMaxLength(256);
        
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.Property(x => x.Status)
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(512);
        
        // Many-to-one relationship with VotingResult
        builder.HasOne(x => x.VotingResult)
            .WithMany()
            .HasForeignKey(x => x.VotingResultId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(x => x.VotingResultId);
        builder.HasIndex(x => x.VotingId);
        builder.HasIndex(x => x.TransactionHash)
            .IsUnique();
        builder.HasIndex(x => new { x.Network, x.Status });
        builder.HasIndex(x => x.CreatedAt);
    }
}
