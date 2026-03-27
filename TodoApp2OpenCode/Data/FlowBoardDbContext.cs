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
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<CalendarEvent> CalendarEvents { get; set; } = null!;

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
            entity.Property(e => e.ParticipantsJson)
                .HasColumnType("nvarchar(max)");
            
            entity.HasMany(b => b.Columns)
                .WithOne()
                .HasForeignKey(c => c.BoardId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(b => b.Items)
                .WithOne()
                .HasForeignKey(i => i.TodoBoardId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(b => b.Events)
                .WithOne()
                .HasForeignKey(e => e.TodoBoardId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.HasMany(i => i.Steps)
                .WithOne()
                .HasForeignKey(s => s.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username);
        });
    }
}
