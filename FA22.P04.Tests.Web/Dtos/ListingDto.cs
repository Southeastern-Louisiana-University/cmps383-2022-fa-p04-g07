namespace FA22.P04.Tests.Web.Dtos;

internal class ListingDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public DateTimeOffset? StartUtc { get; set; }
    public DateTimeOffset? EndUtc { get; set; }
}