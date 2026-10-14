using CivicConnect.Domain.Entities;
using CivicConnect.Domain.Enums;
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
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // M3-DATA-01 (Issue #36): indexes for dashboard and queue queries.
        modelBuilder.Entity<Request>().HasIndex(r => r.State);
        modelBuilder.Entity<Request>().HasIndex(r => r.RequesterId);
        modelBuilder.Entity<Request>().HasIndex(r => r.AssigneeId);
        modelBuilder.Entity<Request>().HasIndex(r => r.CreatedAt);
        modelBuilder.Entity<Request>().HasIndex(r => new { r.State, r.CategoryId });
        modelBuilder.Entity<Request>().HasIndex(r => new { r.AssigneeId, r.State });

        // NFR-020 (Issue #35): audit read path (manager queries by entity + time).
        modelBuilder.Entity<AuditLog>().HasIndex(a => new { a.EntityType, a.EntityId });
        modelBuilder.Entity<AuditLog>().HasIndex(a => a.Timestamp);
        
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Infrastructure", Description = "Roads, bridges, etc." },
            new Category { Id = 2, Name = "Utilities", Description = "Water, electricity, etc." },
            new Category { Id = 3, Name = "Parks", Description = "Public parks and recreation" }
        );

        modelBuilder.Entity<Request>().HasData(
            new Request
            {
                Id = 2,
                Title = "Pothole on Main Street",
                Description = "A large pothole is affecting traffic near the community center.",
                CategoryId = 1,
                State = RequestState.Open,
                CreatedAt = new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc),
                RequesterId = "requester-2",
                AssigneeId = null
            }
        );

        modelBuilder.Entity<Comment>().HasData(
            new Comment
            {
                Id = 1,
                RequestId = 2,
                Text = "Thank you for reporting this issue. Our team will investigate it.",
                CreatedAt = new DateTime(2026, 10, 1, 9, 0, 0, DateTimeKind.Utc),
                CreatedBy = "staff-1",
                IsInternal = false
            }
        );
    }
}
