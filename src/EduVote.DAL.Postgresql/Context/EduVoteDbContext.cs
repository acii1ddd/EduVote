using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using Microsoft.EntityFrameworkCore;

namespace EduVote.DAL.Postgresql.Context;

public class EduVoteDbContext(DbContextOptions<EduVoteDbContext> options) 
    : DbContext(options)
{
    public DbSet<Voting> Votings { get; set; }
    
    public DbSet<Candidate> Candidates { get; set; }

    public DbSet<Vote> Votes { get; set; }
    
    public DbSet<User> Users { get; set; }
    
    public DbSet<Role> Roles { get; set; }
    
    public DbSet<VotingTarget> VotingTargets { get; set; }

    public DbSet<EducationUnit> EducationUnits { get; set; }
    
    public DbSet<UserEducationUnit> UserEducationUnits { get; set; }
    
    public DbSet<VotingResult> VotingResults { get; set; }
    
    public DbSet<BlockchainRecord> BlockchainRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EduVoteDbContext).Assembly);
    }
}