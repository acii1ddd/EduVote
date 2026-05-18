using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduVote.DAL.Postgresql.ModelConfigurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    private const int MaxLength = 128;
    
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .HasMaxLength(MaxLength)
            .IsRequired();
        
        builder.HasMany(x => x.Users)
            .WithOne(x => x.UserRole)
            .HasForeignKey(x => x.RoleId);
    }
}