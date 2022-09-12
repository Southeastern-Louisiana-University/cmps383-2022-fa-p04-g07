using FA22.P04.Web.Features.Products;
using Microsoft.EntityFrameworkCore;

namespace FA22.P04.Web.Data;

public static class MigrateAndSeed
{
    public static void Initialize(IServiceProvider services)
    {
        var context = services.GetRequiredService<DataContext>();
        context.Database.Migrate();

        AddProducts(context);
    }

    private static void AddProducts(DataContext context)
    {
        var products = context.Set<Product>();
        if (products.Any())
        {
            return;
        }

        products.Add(new Product
        {
            Name = "Super Mario World",
            Description = "Super Nintendo (SNES) System",
        });
        products.Add(new Product
        {
            Name = "Donkey Kong 64",
            Description = "Donkey Kong 64 cartridge for the Nintendo 64",
        });
        products.Add(new Product
        {
            Name = "Half-Life 2: Collector's Edition",
            Description = "PC platform release of the 2004 wonder",
        });
        context.SaveChanges();
    }
}