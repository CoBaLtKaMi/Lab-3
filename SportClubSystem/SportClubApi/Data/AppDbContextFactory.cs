using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SportClubApi.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var opt = new DbContextOptionsBuilder<AppDbContext>();
        opt.UseNpgsql("Host=localhost;Database=sportclub;Username=postgres;Password=postgres");
        return new AppDbContext(opt.Options);
    }
}
