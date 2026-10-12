using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CivicConnect.Domain.Enums;
using CivicConnect.Web.Controllers;
using Xunit;
using CivicConnect.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using CivicConnect.Web.Data;

namespace CivicConnect.Tests;

public class RequestControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public RequestControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
    }

    [Fact]
    public async Task UpdateState_ClosedToOpen_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient().AsStaff();
        var db = _factory.CreateDbContext();

        var category = new Category { Name = "TestCategory" };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var request = new Request { CategoryId = category.Id, Title = "Title", Description = "Desc", State = RequestState.Closed };
        db.Requests.Add(request);
        await db.SaveChangesAsync();

        var updateDto = new UpdateStateDto { NewState = RequestState.Open, Reason = "Should fail" };

        // Act
        var response = await client.PutAsJsonAsync($"/api/requests/{request.Id}/state", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
