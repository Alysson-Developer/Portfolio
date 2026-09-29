using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Portfolio.Data;

public sealed class PortfolioDbContextFactory : IDesignTimeDbContextFactory<PortfolioDbContext>
{
    public PortfolioDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PortfolioDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=design-time;Port=5432;Database=portfolio;Username=postgres;Password=postgres");

        return new PortfolioDbContext(optionsBuilder.Options);
    }
}