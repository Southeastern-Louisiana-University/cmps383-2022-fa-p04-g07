using Microsoft.EntityFrameworkCore;

namespace FA22.P04.Web.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DataContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // this stores all decimal values to two decimal points by default - good enough for our purposes
        configurationBuilder.Properties<decimal>()
            .HavePrecision(18, 2);
    }
}
