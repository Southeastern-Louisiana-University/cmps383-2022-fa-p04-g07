using System.Net;
using FA22.P04.Tests.Web.Controllers.Authentication;
using FA22.P04.Tests.Web.Controllers.Products;
using FA22.P04.Tests.Web.Dtos;
using FA22.P04.Tests.Web.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FA22.P04.Tests.Web.Controllers.Items;

[TestClass]
public class ItemsControllerTests
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
    public async Task CreateItem_NoProductId_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsBob();
        var request = new ItemDto
        {
            ProductId = null,
            Condition = "Good",
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/items", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/items with no name");
    }

    [TestMethod]
    public async Task CreateItem_InvalidProductId_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsBob();
        var request = new ItemDto
        {
            ProductId = 9999,
            Condition = "Good",
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/items", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/items with an invalid product id");
    }

    [TestMethod]
    public async Task CreateItem_NoCondition_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var productDto = await webClient.GetProduct();
        if (productDto == null)
        {
            Assert.Fail("You are not ready for this test - make product listing work first");
        }
        await webClient.AssertLoggedInAsBob();
        var request = new ItemDto
        {
            ProductId = productDto.Id,
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/items", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/items with no condition");
    }

    [TestMethod]
    public async Task CreateItem_Valid_Returns201()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var productDto = await webClient.GetProduct();
        if (productDto == null)
        {
            Assert.Fail("You are not ready for this test - make product listing work first");
        }
        await webClient.AssertLoggedInAsBob();
        var request = new ItemDto
        {
            ProductId = productDto.Id,
            Condition = "Good"
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/items", request);

        //assert
        await httpResponse.AssertCreateItemFunctions(request, webClient);
    }

    [TestMethod]
    public async Task UpdateItem_NoProductId_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var productDto = await webClient.GetProduct();
        if (productDto == null)
        {
            Assert.Fail("You are not ready for this test - make product listing work first");
        }
        var request = new ItemDto
        {
            ProductId = productDto.Id,
            Condition = "Good"
        };
        await using var handle = await webClient.CreateItem(request);
        if (handle == null)
        {
            Assert.Fail("You are not ready for this test - make item create work first");
        }
        await webClient.AssertLoggedInAsBob();

        request.ProductId = null;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/items/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/items/{id} with a missing product id");
    }

    [TestMethod]
    public async Task UpdateItem_BadProductId_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var productDto = await webClient.GetProduct();
        if (productDto == null)
        {
            Assert.Fail("You are not ready for this test - make product listing work first");
        }
        var request = new ItemDto
        {
            ProductId = productDto.Id,
            Condition = "Good"
        };
        await using var handle = await webClient.CreateItem(request);
        if (handle == null)
        {
            Assert.Fail("You are not ready for this test - make item create work first");
        }
        await webClient.AssertLoggedInAsBob();

        request.ProductId = 9999;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/items/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/items/{id} with a invalid product id");
    }

    [TestMethod]
    public async Task UpdateItem_NoCondition_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var productDto = await webClient.GetProduct();
        if (productDto == null)
        {
            Assert.Fail("You are not ready for this test - make product listing work first");
        }
        var request = new ItemDto
        {
            ProductId = productDto.Id,
            Condition = "Good"
        };
        await using var handle = await webClient.CreateItem(request);
        if (handle == null)
        {
            Assert.Fail("You are not ready for this test - make item create work first");
        }
        await webClient.AssertLoggedInAsBob();

        request.Condition = null;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/items/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/items/{id} with a missing product id");
    }

    [TestMethod]
    public async Task UpdateItem_Valid_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var productDto = await webClient.GetProduct();
        if (productDto == null)
        {
            Assert.Fail("You are not ready for this test - make product listing work first");
        }
        var request = new ItemDto
        {
            ProductId = productDto.Id,
            Condition = "Good"
        };
        await using var handle = await webClient.CreateItem(request);
        if (handle == null)
        {
            Assert.Fail("You are not ready for this test - make item create work first");
        }
        await webClient.AssertLoggedInAsBob();

        request.Condition = "Bad";

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/items/{request.Id}", request);

        //assert
        await httpResponse.AssertItemUpdateFunctions(request, webClient);
    }

    [TestMethod]
    public async Task DeleteItem_Valid_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var productDto = await webClient.GetProduct();
        if (productDto == null)
        {
            Assert.Fail("You are not ready for this test - make product listing work first");
        }
        var request = new ItemDto
        {
            ProductId = productDto.Id,
            Condition = "Good"
        };
        await using var handle = await webClient.CreateItem(request);
        if (handle == null)
        {
            Assert.Fail("You are not ready for this test - make item create work first");
        }
        await webClient.AssertLoggedInAsBob();

        //act
        var httpResponse = await webClient.DeleteAsync($"/api/items/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling DELETE /api/items/{id} with a valid id");
    }

    [TestMethod]
    public async Task DeleteItem_NotLoggedIn_Returns401()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var productDto = await webClient.GetProduct();
        if (productDto == null)
        {
            Assert.Fail("You are not ready for this test - make product listing work first");
        }
        var request = new ItemDto
        {
            ProductId = productDto.Id,
            Condition = "Good"
        };
        await using var handle = await webClient.CreateItem(request);
        if (handle == null)
        {
            Assert.Fail("You are not ready for this test - make item create work first");
        }

        //act
        var httpResponse = await webClient.DeleteAsync($"/api/items/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "we expect an HTTP 401 when calling DELETE /api/items/{id} when not logged in");
    }

    [TestMethod]
    public async Task DeleteItem_LoggedInAsOtherUser_Returns403()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var productDto = await webClient.GetProduct();
        if (productDto == null)
        {
            Assert.Fail("You are not ready for this test - make product listing work first");
        }
        var request = new ItemDto
        {
            ProductId = productDto.Id,
            Condition = "Good"
        };
        await using var handle = await webClient.CreateItem(request);
        if (handle == null)
        {
            Assert.Fail("You are not ready for this test - make item create work first");
        }
        await webClient.LoginAsSueAsync();

        //act
        var httpResponse = await webClient.DeleteAsync($"/api/items/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, "we expect an HTTP 403 when calling DELETE /api/items/{id} when logged in as wrong user");
    }

    [TestMethod]
    public async Task DeleteItem_Admin_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var productDto = await webClient.GetProduct();
        if (productDto == null)
        {
            Assert.Fail("You are not ready for this test - make product listing work first");
        }
        var request = new ItemDto
        {
            ProductId = productDto.Id,
            Condition = "Good"
        };
        await using var handle = await webClient.CreateItem(request);
        if (handle == null)
        {
            Assert.Fail("You are not ready for this test - make item create work first");
        }
        await webClient.AssertLoggedInAsAdmin();

        //act
        var httpResponse = await webClient.DeleteAsync($"/api/items/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling DELETE /api/items/{id} with a valid id");
    }

    [TestMethod]
    public async Task DeleteItem_TwiceOnSameId_Returns404()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var productDto = await webClient.GetProduct();
        if (productDto == null)
        {
            Assert.Fail("You are not ready for this test - make product listing work first");
        }
        var request = new ItemDto
        {
            ProductId = productDto.Id,
            Condition = "Good"
        };
        await using var handle = await webClient.CreateItem(request);
        if (handle == null)
        {
            Assert.Fail("You are not ready for this test - make item create work first");
        }
        await webClient.AssertLoggedInAsBob();

        //act
        await webClient.DeleteAsync($"/api/items/{request.Id}");
        var httpResponse = await webClient.DeleteAsync($"/api/items/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling DELETE /api/items/{id} with a valid id twice in a row");
    }

    [TestMethod]
    public async Task UpdateItem_NotLoggedIn_Returns401()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var productDto = await webClient.GetProduct();
        if (productDto == null)
        {
            Assert.Fail("You are not ready for this test - make product listing work first");
        }
        var request = new ItemDto
        {
            ProductId = productDto.Id,
            Condition = "Good"
        };
        await using var handle = await webClient.CreateItem(request);
        if (handle == null)
        {
            Assert.Fail("You are not ready for this test - make item create work first");
        }

        request.Condition = "Bad";

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/items/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "we expect an HTTP 401 when calling DELETE /api/items/{id} when not logged in");
    }

    [TestMethod]
    public async Task UpdateItem_LoggedInAsOtherUser_Returns403()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var productDto = await webClient.GetProduct();
        if (productDto == null)
        {
            Assert.Fail("You are not ready for this test - make product listing work first");
        }
        var request = new ItemDto
        {
            ProductId = productDto.Id,
            Condition = "Good"
        };
        await using var handle = await webClient.CreateItem(request);
        if (handle == null)
        {
            Assert.Fail("You are not ready for this test - make item create work first");
        }
        await webClient.AssertLoggedInAsSue();

        request.Condition = "Bad";

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/items/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, "we expect an HTTP 403 when calling DELETE /api/items/{id} when logged in as wrong user");
    }

}
