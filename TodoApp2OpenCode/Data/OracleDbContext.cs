using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Data;

public class OracleDbContext : DbContext, IFlowBoardDbContext
{
    private static readonly ValueConverter<bool, int> BoolToIntConverter = new(
        v => v ? 1 : 0,
        v => v == 1);

    private static readonly ValueComparer<bool> BoolComparer = new(
        (v1, v2) => v1 == v2,
        v => v.GetHashCode());

    public OracleDbContext(DbContextOptions<OracleDbContext> options) : base(options)
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

        modelBuilder.Entity<TodoBoard>(entity =>
        {
            entity.Property(e => e.Description)
                .HasColumnType("NVARCHAR2(500)")
                .IsRequired(false);

            entity.Property(e => e.OwnerName)
                .HasColumnType("NVARCHAR2(100)")
                .IsRequired(false);

            entity.Property(e => e.ParticipantsJson)
                .HasColumnType("CLOB");

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
            entity.Property(e => e.IsCompleted).HasConversion(BoolToIntConverter);

            entity.HasMany(i => i.Steps)
                .WithOne()
                .HasForeignKey(s => s.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TodoStep>(entity =>
        {
            entity.Property(e => e.IsCompleted).HasConversion(BoolToIntConverter);
            entity.HasIndex(s => new { s.ItemId, s.Order });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username);
        });
    }
}
