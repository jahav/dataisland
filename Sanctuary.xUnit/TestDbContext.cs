using Microsoft.EntityFrameworkCore;

namespace Sanctuary.xUnit;

public class QueryDbContext : DbContext
{
    public QueryDbContext(DbContextOptions<QueryDbContext> options) : base(options)
    {
            
    }

    public required DbSet<Users> Users { get; init; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Users>(entityBuilder =>
        {
            entityBuilder.HasKey(e => e.UserId);
        });
    }
}

public class Users
{
    public required Guid UserId { get; set; }
    
    public required string Name { get; set; }
}