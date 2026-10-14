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
        const string requesterId = "requester-1";
        var client = _factory.CreateClient().AsStaff(requesterId);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();

        var request = new Request
        {
            CategoryId = 1,
            CreatedAt = DateTime.UtcNow,
            Title = "Title",
            Description = "Desc",
            RequesterId = requesterId,
            State = RequestState.Closed
        };
        db.Requests.Add(request);
        await db.SaveChangesAsync();

        var updateDto = new UpdateStateDto { NewState = RequestState.Open, Reason = "Should fail" };

        // Act
        var response = await client.PutAsJsonAsync($"/api/requests/{request.Id}/state", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
