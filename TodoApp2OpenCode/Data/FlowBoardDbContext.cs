using Microsoft.EntityFrameworkCore;
using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Data;

public class FlowBoardDbContext : DbContext
{
    public FlowBoardDbContext(DbContextOptions<FlowBoardDbContext> options) : base(options)
    {
    }

    public DbSet<TestEntity> TestEntities { get; set; } = null!;
    public DbSet<LogItem> LogItems { get; set; } = null!;
    public DbSet<TodoBoard> Boards { get; set; } = null!;
    public DbSet<TodoColumn> Columns { get; set; } = null!;
    public DbSet<TodoItem> Items { get; set; } = null!;
    public DbSet<TodoStep> Steps { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestEntity>().HasData(
            new TestEntity { Id = 1, Name = "Primer Registro", Description = "Este es el primer registro de prueba" },
            new TestEntity { Id = 2, Name = "Segundo Registro", Description = "Segundo registro para verificar la conexión" },
            new TestEntity { Id = 3, Name = "Tercer Registro", Description = "Tercer registro - si ves esto, la conexión funciona" }
        );

        modelBuilder.Entity<TodoBoard>(entity =>
        {
            entity.Property(e => e.ParticipantIds)
                .HasConversion(
                    v => string.Join(",", v),
                    v => string.IsNullOrEmpty(v) ? new List<string>() : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
            
            entity.Property(e => e.ParticipantNamesJson)
                .HasColumnType("nvarchar(max)");
        });
    }
}
