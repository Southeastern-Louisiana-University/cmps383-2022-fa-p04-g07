using System.Net;
using FA22.P04.Tests.Web.Controllers.Authentication;
using FA22.P04.Tests.Web.Controllers.Items;
using FA22.P04.Tests.Web.Controllers.Products;
using FA22.P04.Tests.Web.Dtos;
using FA22.P04.Tests.Web.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FA22.P04.Tests.Web.Controllers.Listings;

[TestClass]
public class ListingsControllerTests
{
    private WebTestContext context = new();

    [TestInitialize]
    public void Init()
    {
        context = new WebTestContext();
    }

    [TestCleanup]
    public void Cleanup()
    {
        context.Dispose();
    }

    [TestMethod]
    public async Task CreateListing_Valid_Return201()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ListingDto
        {
            Name = "Good games",
            Description = "Stuff",
            Price = 999,
            StartUtc = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(2)),
            EndUtc = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1)),
        };
        await webClient.AssertLoggedInAsBob();

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/listings/", request);

        //assert
        await httpResponse.AssertCreateListingFunctions(request, webClient);
    }

    [TestMethod]
    public async Task CreateListing_NotLoggedIn_Return401()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ListingDto
        {
            Name = "Good games",
            Description = "Stuff",
            Price = 999,
            StartUtc = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(2)),
            EndUtc = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1)),
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/listings/", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "we expect an HTTP 401 when calling POST /api/listings when not logged in");
    }

    [TestMethod]
    public async Task CreateListing_ValidAndActive_Returns201()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ListingDto
        {
            Name = "Good games",
            Description = "Stuff",
            Price = 999,
            StartUtc = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1)),
            EndUtc = DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(1)),
        };
        await webClient.AssertLoggedInAsBob();

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/listings/", request);

        //assert
        await httpResponse.AssertCreateListingFunctions(request, webClient);
    }

    [TestMethod]
    public async Task GetListingById_InvalidId_Returns404()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsBob();

        //act
        var httpResponse = await webClient.GetAsync("/api/listings/9999");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling GET /api/listings/{id} with an invalid id");
    }

    [TestMethod]
    public async Task CreateListing_EndBeforeStart_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ListingDto
        {
            Name = "Good games",
            Description = "Stuff",
            Price = 999,
            EndUtc = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1)),
            StartUtc = DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(1)),
        };
        await webClient.AssertLoggedInAsBob();

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/listings/", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/listings with a listing that has a start date after the end date");
    }

    [TestMethod]
    public async Task DeleteListing_Valid_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ListingDto
        {
            Name = "Good games",
            Description = "Stuff",
            Price = 999,
            StartUtc = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(2)),
            EndUtc = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1)),
        };
        await using var handle = await webClient.CreateListing(request);
        if (handle == null)
        {
            Assert.Fail("You are not ready for this test");
        }
        await webClient.AssertLoggedInAsBob();

        //act
        var httpResponse = await webClient.DeleteAsync($"/api/listings/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling DELETE /api/listings/{id} with a valid id");
    }

    [TestMethod]
    public async Task DeleteListing_NotLoggedIn_Returns401()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ListingDto
        {
            Name = "Good games",
            Description = "Stuff",
            Price = 999,
            StartUtc = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(2)),
            EndUtc = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1)),
        };
        await using var handle = await webClient.CreateListing(request);
        if (handle == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        //act
        var httpResponse = await webClient.DeleteAsync($"/api/listings/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "we expect an HTTP 401 when calling DELETE /api/listings/{id} when not logged in");
    }

    [TestMethod]
    public async Task DeleteListing_WrongUserLoggedIn_Returns403()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ListingDto
        {
            Name = "Good games",
            Description = "Stuff",
            Price = 999,
            StartUtc = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(2)),
            EndUtc = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1)),
        };
        await using var handle = await webClient.CreateListing(request);
        if (handle == null)
        {
            Assert.Fail("You are not ready for this test");
        }
        await webClient.AssertLoggedInAsSue();

        //act
        var httpResponse = await webClient.DeleteAsync($"/api/listings/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, "we expect an HTTP 403 when calling DELETE /api/listings/{id} when logged in as wrong user");
    }

    [TestMethod]
    public async Task DeleteListing_SameIdTwice_Returns404()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ListingDto
        {
            Name = "Good games",
            Description = "Stuff",
            Price = 999,
            StartUtc = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(2)),
            EndUtc = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1)),
        };
        await using var handle = await webClient.CreateListing(request);
        if (handle == null)
        {
            Assert.Fail("You are not ready for this test");
        }
        await webClient.AssertLoggedInAsBob();

        //act
        await webClient.DeleteAsync($"/api/listings/{request.Id}");
        var httpResponse = await webClient.DeleteAsync($"/api/listings/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling DELETE /api/listings/{id} with the same id twice");
    }

    [TestMethod]
    public async Task SetListingItems_AllProducts_Returns204()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var listingDto = new ListingDto
        {
            Name = "Good games",
            Description = "Stuff",
            Price = 999,
            StartUtc = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(2)),
            EndUtc = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1)),
        };
        await using var handle = await webClient.CreateListing(listingDto);
        if (handle == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        var products = await webClient.GetProducts();
        if (products == null || !products.Any())
        {
            Assert.Fail("You are not ready for this test");
        }

        var items = new List<ItemDto>();
        foreach (var productDto in products)
        {
            var item = new ItemDto
            {
                Condition = "GReat",
                ProductId = productDto.Id
            };
            if (await webClient.CreateItem(item) == null)
            {
                Assert.Fail("You are not ready for this test");
            }
            items.Add(new ItemDto
            {
                Id = item.Id
            });
        }
        await webClient.AssertLoggedInAsBob();

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/listings/{listingDto.Id}/items", items);

        //assert
        await httpResponse.AssertSetListingItemsFunctions(items, listingDto, webClient);
    }

    [TestMethod]
    public async Task SetListingItems_ActiveListingAndAllProducts_Returns204()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var listingDto = new ListingDto
        {
            Name = "Good games",
            Description = "Stuff",
            Price = 999,
            StartUtc = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(2)),
            EndUtc = DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(1)),
        };
        await using var handle = await webClient.CreateListing(listingDto);
        if (handle == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        var products = await webClient.GetProducts();
        if (products == null || !products.Any())
        {
            Assert.Fail("You are not ready for this test");
        }

        var items = new List<ItemDto>();
        foreach (var productDto in products)
        {
            var item = new ItemDto
            {
                Condition = "GReat",
                ProductId = productDto.Id
            };
            if (await webClient.CreateItem(item) == null)
            {
                Assert.Fail("You are not ready for this test");
            }
            items.Add(new ItemDto
            {
                Id = item.Id
            });
        }
        await webClient.AssertLoggedInAsBob();

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/listings/{listingDto.Id}/items", items);

        //assert
        await httpResponse.AssertSetListingItemsFunctions(items, listingDto, webClient);
    }

    [TestMethod]
    public async Task SetListingItems_TwoDifferntSets_LastOneWins()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var listingDto = new ListingDto
        {
            Name = "Good games",
            Description = "Stuff",
            Price = 999,
            StartUtc = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(2)),
            EndUtc = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1)),
        };
        await using var handle = await webClient.CreateListing(listingDto);
        if (handle == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        var products = await webClient.GetProducts();
        if (products == null || !products.Any())
        {
            Assert.Fail("You are not ready for this test");
        }

        var items = new List<ItemDto>();
        foreach (var productDto in products)
        {
            var item = new ItemDto
            {
                Condition = "GReat",
                ProductId = productDto.Id
            };
            if (await webClient.CreateItem(item) == null)
            {
                Assert.Fail("You are not ready for this test");
            }
            items.Add(new ItemDto
            {
                Id = item.Id
            });
        }
        await webClient.AssertLoggedInAsBob();

        //first time
        var itemsA = items.Take(1).ToList();
        var httpResponseA = await webClient.PutAsJsonAsync($"/api/listings/{listingDto.Id}/items", itemsA);
        await httpResponseA.AssertSetListingItemsFunctions(itemsA, listingDto, webClient);

        //second time
        var itemsB = items.Skip(1).ToList();
        var httpResponseB = await webClient.PutAsJsonAsync($"/api/listings/{listingDto.Id}/items", itemsB);
        await httpResponseB.AssertSetListingItemsFunctions(itemsB, listingDto, webClient);
    }
}
