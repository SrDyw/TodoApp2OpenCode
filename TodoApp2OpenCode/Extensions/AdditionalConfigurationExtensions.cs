using Microsoft.EntityFrameworkCore;
using TodoApp2OpenCode.Configurations;
using TodoApp2OpenCode.Data;

namespace TodoApp2OpenCode.Extensions
{
    public static class AdditionalConfigurationExtensions
    {
        public static void AddDatabaseProviderConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            switch (DatabaseProvider.Value)
            {
                case Models.DatabaseProviderName.SqlServer:
                    services.AddSqlSeverConfigurations(configuration);
                    break;
                case Models.DatabaseProviderName.Oracle:
                    services.AddOracleConfigurations(configuration);
                    break;

            }
        }


        public static void AddSqlSeverConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContextFactory<FlowBoardDbContext>(options =>
             {
                 options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                        .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.MultipleCollectionIncludeWarning));
             });

        }

        public static void AddOracleConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContextFactory<FlowBoardDbContext>(options =>
            {
                options.UseOracle(configuration.GetConnectionString("OracleConnection"))
                       .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.MultipleCollectionIncludeWarning));
            });

        }
    }
}
