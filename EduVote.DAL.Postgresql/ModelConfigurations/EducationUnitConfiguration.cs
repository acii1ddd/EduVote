using EduVote.DAL.Postgresql.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduVote.DAL.Postgresql.ModelConfigurations;

public class EducationUnitConfiguration : IEntityTypeConfiguration<EducationUnit>
{
    private const int MaxLength = 128;
    
    public void Configure(EntityTypeBuilder<EducationUnit> builder)
    {
        builder.ToTable("EducationUnits");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .HasMaxLength(MaxLength)
            .IsRequired();
        
        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(x => new {x.Name, x.ParentId})
            .IsUnique();

        builder.HasMany(x => x.VotingTargets)
            .WithOne(x => x.EducationUnit)
            .HasForeignKey(x => x.EducationUnitId);
        
        // UserEducationUnit
        builder.HasMany(x => x.UserEducationUnits)
            .WithOne(x => x.EducationUnit)
            .HasForeignKey(x => x.EducationUnitId);
    }
}