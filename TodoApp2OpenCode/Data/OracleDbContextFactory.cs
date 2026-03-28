using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TodoApp2OpenCode.Data;

public class OracleDbContextFactory : IDesignTimeDbContextFactory<OracleDbContext>
{
    public OracleDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OracleDbContext>();
        optionsBuilder.UseOracle("USER ID=aduana_test;PASSWORD=gesti;DATA SOURCE=(DESCRIPTION =(ADDRESS_LIST =(ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.8.23)(PORT = 1521)))(CONNECT_DATA =(SERVICE_NAME = orcl)))");
        return new OracleDbContext(optionsBuilder.Options);
    }
}
