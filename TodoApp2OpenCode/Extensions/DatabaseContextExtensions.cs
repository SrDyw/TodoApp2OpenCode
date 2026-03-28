using Microsoft.EntityFrameworkCore;
using TodoApp2OpenCode.Configurations;

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
                    break;
            }
        }
    }
}
