using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CafeCrm.Infrastructure.Persistence;

public class DesignTimeCafeCrmDbContextFactory : IDesignTimeDbContextFactory<CafeCrmDbContext>
{
    public CafeCrmDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<CafeCrmDbContext>();
        var basePath = Directory.GetCurrentDirectory();
        var dataDirectory = Path.GetFullPath(Path.Combine(basePath, "Data"));
        Directory.CreateDirectory(dataDirectory);
        var dbPath = Path.Combine(dataDirectory, "cafecrm.db");
        builder.UseSqlite($"Data Source={dbPath}");
        return new CafeCrmDbContext(builder.Options);
    }
}
