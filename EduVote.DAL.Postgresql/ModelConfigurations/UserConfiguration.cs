using EduVote.DAL.Postgresql.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduVote.DAL.Postgresql.ModelConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    private const int MaxLength = 128;
    
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Email)
            .HasMaxLength(MaxLength)
            .IsRequired();
        
        builder.Property(x => x.RoleId).IsRequired();

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(MaxLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
        
        builder.HasOne(x => x.UserRole)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.RoleId);
        
        builder.HasMany(x => x.Votes)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);
        
        // UserEducationUnit
        builder.HasMany(x => x.UserEducationUnits)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);
    }
}