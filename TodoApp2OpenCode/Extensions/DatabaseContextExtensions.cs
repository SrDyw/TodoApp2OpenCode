using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TodoApp2OpenCode.Configurations;
using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Extensions
{
    public static class DatabaseContextExtensions
    {
        public static void AddProviderAdditionalConfigurations(this ModelBuilder modelBuilder)
        {
            switch(DatabaseProvider.Value)
            {
                case Models.DatabaseProviderName.SqlServer:
                    break;
                case Models.DatabaseProviderName.Oracle:
                    var boolToIntValueConverter = new ValueConverter<bool, int>(
                        v => v ? 1 : 0,
                        v => v != 0);

                    modelBuilder.Entity<TodoItem>(entity =>
                    {
                        entity.Property(e => e.IsCompleted)
                            .HasConversion(boolToIntValueConverter);
                    });

                    modelBuilder.Entity<TodoStep>(entity =>
                    {
                        entity.Property(e => e.IsCompleted)
                            .HasConversion(boolToIntValueConverter);
                    });
                    break;
            }
        }
    }
}
