using CivicConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CivicConnect.Web.Data;

public class CivicConnectDbContext : DbContext
{
    public CivicConnectDbContext(DbContextOptions<CivicConnectDbContext> options) : base(options)
    {
    }

    public DbSet<Request> Requests => Set<Request>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<StatusHistory> StatusHistories => Set<StatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Infrastructure", Description = "Roads, bridges, etc." },
            new Category { Id = 2, Name = "Utilities", Description = "Water, electricity, etc." },
            new Category { Id = 3, Name = "Parks", Description = "Public parks and recreation" }
        );
    }
}
