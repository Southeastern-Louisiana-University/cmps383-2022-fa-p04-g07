using System.Net;
using FA22.P04.Tests.Web.Controllers.Authentication;
using FA22.P04.Tests.Web.Dtos;
using FA22.P04.Tests.Web.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FA22.P04.Tests.Web.Controllers.Products;

[TestClass]
public class ProductsControllerTests
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
    public async Task ListAllProducts_Returns200AndData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var httpResponse = await webClient.GetAsync("/api/products");

        //assert
        await httpResponse.AssertProductListAllFunctions();
    }

    [TestMethod]
    public async Task GetProductById_Returns200AndData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = await webClient.GetProduct();
        if (target == null)
        {
            Assert.Fail("Make List All products work first");
            return;
        }

        //act
        var httpResponse = await webClient.GetAsync($"/api/products/{target.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling GET /api/products/{id} ");
        var resultDto = await httpResponse.Content.ReadAsJsonAsync<ProductDto>();
        resultDto.Should().BeEquivalentTo(target, "we expect get product by id to return the same data as the list all product endpoint");
    }

    [TestMethod]
    public async Task GetProductById_NoSuchId_Returns404()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var httpResponse = await webClient.GetAsync("/api/products/999999");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling GET /api/products/{id} with an invalid id");
    }

    [TestMethod]
    public async Task CreateProduct_NoName_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsAdmin();
        var request = new ProductDto
        {
            Description = "asd",
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/products", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/products with no name");
    }

    [TestMethod]
    public async Task CreateProduct_NameTooLong_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsAdmin();
        var request = new ProductDto
        {
            Name = "a".PadLeft(121, '0'),
            Description = "asd",
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/products", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/products with a name that is too long");
    }

    [TestMethod]
    public async Task CreateProduct_NoDescription_ReturnsError()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsAdmin();
        var target = await webClient.GetProduct();
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        var request = new ProductDto
        {
            Name = "asd",
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/products", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/products with no description");
    }

    [TestMethod]
    public async Task CreateProduct_Returns201AndData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsAdmin();
        var request = new ProductDto
        {
            Name = "a",
            Description = "asd",
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/products", request);

        //assert
        await httpResponse.AssertCreateProductFunctions(request, webClient);
    }

    [TestMethod]
    public async Task CreateProduct_NotLoggedIn_Returns401()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ProductDto
        {
            Name = "a",
            Description = "asd",
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/products", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "we expect an HTTP 401 when calling POST /api/products when not logged in");
    }

    [TestMethod]
    public async Task CreateProduct_LoggedInAsBob_Returns403()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsBob();
        var request = new ProductDto
        {
            Name = "a",
            Description = "asd",
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/products", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, "we expect an HTTP 403 when calling POST /api/products when logged in as bob");
    }

    [TestMethod]
    public async Task UpdateProduct_NoName_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ProductDto
        {
            Name = "a",
            Description = "desc",
        };
        await using var target = await webClient.CreateProduct(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }
        await webClient.AssertLoggedInAsAdmin();

        request.Name = null;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/products/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/products/{id} with a missing name");
    }

    [TestMethod]
    public async Task UpdateProduct_NameTooLong_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ProductDto
        {
            Name = "a",
            Description = "desc",
        };
        await using var target = await webClient.CreateProduct(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }
        await webClient.AssertLoggedInAsAdmin();

        request.Name = "a".PadLeft(121, '0');

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/products/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/products/{id} with a name that is too long");
    }

    [TestMethod]
    public async Task UpdateProduct_NoDescription_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ProductDto
        {
            Name = "a",
            Description = "desc",
        };
        await using var target = await webClient.CreateProduct(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }
        await webClient.AssertLoggedInAsAdmin();

        request.Description = null;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/products/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/products/{id} with a missing description");
    }

    [TestMethod]
    public async Task UpdateProduct_Valid_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ProductDto
        {
            Name = "a",
            Description = "desc",
        };
        await using var target = await webClient.CreateProduct(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }
        await webClient.AssertLoggedInAsAdmin();

        request.Description = "cool new description";

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/products/{request.Id}", request);

        //assert
        await httpResponse.AssertProductUpdateFunctions(request, webClient);
    }

    [TestMethod]
    public async Task UpdateProduct_NotLoggedIn_Returns401()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ProductDto
        {
            Name = "a",
            Description = "desc",
        };
        await using var target = await webClient.CreateProduct(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        request.Description = "cool new description";

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/products/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "we expect an HTTP 401 when calling PUT /api/products/{id} when not logged in");
    }

    [TestMethod]
    public async Task UpdateProduct_LoggedInAsBob_Returns403()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ProductDto
        {
            Name = "a",
            Description = "desc",
        };
        await using var target = await webClient.CreateProduct(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }
        await webClient.AssertLoggedInAsBob();

        request.Description = "cool new description";

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/products/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, "we expect an HTTP 403 when calling PUT /api/products/{id} when logged in as bob");
    }

    [TestMethod]
    public async Task DeleteProduct_NoSuchItem_ReturnsNotFound()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ProductDto
        {
            Description = "asd",
            Name = "asd"
        };
        await using var itemHandle = await webClient.CreateProduct(request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        await webClient.AssertLoggedInAsAdmin();

        //act
        var httpResponse = await webClient.DeleteAsync($"/api/products/{request.Id + 21}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling DELETE /api/products/{id} with an invalid Id");
    }

    [TestMethod]
    public async Task DeleteProduct_ValidItem_ReturnsOk()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ProductDto
        {
            Description = "asd",
            Name = "asd",
        };
        await using var itemHandle = await webClient.CreateProduct(request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        await webClient.AssertLoggedInAsAdmin();

        //act
        var httpResponse = await webClient.DeleteAsync($"/api/products/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling DELETE /api/products/{id} with a valid id");
    }

    [TestMethod]
    public async Task DeleteProduct_NotLoggedIn_Returns401()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ProductDto
        {
            Description = "asd",
            Name = "asd",
        };
        await using var itemHandle = await webClient.CreateProduct(request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }

        //act
        var httpResponse = await webClient.DeleteAsync($"/api/products/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "we expect an HTTP 401 when calling DELETE /api/products/{id} when not logged in");
    }

    [TestMethod]
    public async Task DeleteProduct_LoggedInAsBob_Returns403()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ProductDto
        {
            Description = "asd",
            Name = "asd",
        };
        await using var itemHandle = await webClient.CreateProduct(request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        await webClient.AssertLoggedInAsBob();

        //act
        var httpResponse = await webClient.DeleteAsync($"/api/products/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, "we expect an HTTP 403 when calling DELETE /api/products/{id} when not logged in as an admin");
    }

    [TestMethod]
    public async Task DeleteProduct_SameItemTwice_ReturnsNotFound()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ProductDto
        {
            Description = "asd",
            Name = "asd",
        };
        await using var itemHandle = await webClient.CreateProduct(request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        await webClient.AssertLoggedInAsAdmin();

        //act
        await webClient.DeleteAsync($"/api/products/{request.Id}");
        var httpResponse = await webClient.DeleteAsync($"/api/products/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling DELETE /api/products/{id} on the same item twice");
    }
}
