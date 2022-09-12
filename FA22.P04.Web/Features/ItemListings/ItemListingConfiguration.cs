using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FA22.P04.Web.Features.ItemListings;

public class ItemListingConfiguration : IEntityTypeConfiguration<ItemListing>
{
    public void Configure(EntityTypeBuilder<ItemListing> builder)
    {
    }
}