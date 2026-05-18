using EduVote.DAL.Postgresql.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduVote.DAL.Postgresql.ModelConfigurations;

public class UserEducationUnitConfiguration : IEntityTypeConfiguration<UserEducationUnit>
{
    private const int MaxLength = 512;
    
    public void Configure(EntityTypeBuilder<UserEducationUnit> builder)
    {
        builder.ToTable("UserEducationUnits");
        
        builder.HasKey(x => new { x.UserId, x.EducationUnitId });
        
        // User
        builder.HasOne(x => x.User)
            .WithMany(x => x.UserEducationUnits)
            .HasForeignKey(x => x.UserId);
        
        // EducationUnit
        builder.HasOne(x => x.EducationUnit)
            .WithMany(x => x.UserEducationUnits)
            .HasForeignKey(x => x.EducationUnitId);
    }
}