using System.Net.Http.Json;
using CivicConnect.Domain.Entities;
using CivicConnect.Domain.Enums;
using CivicConnect.Web.Controllers;
using Xunit;

namespace CivicConnect.Tests;

public class GetStaffRequestsScopeTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public GetStaffRequestsScopeTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Queue_ScopesBeforeFiltersAndPaging(bool manager)
    {
        using var db = _factory.CreateDbContext();
        var category = new Category { Name = "Queue scope" };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        foreach (var assignee in new string?[] { null, "staff-a", "staff-b" })
        {
            db.Requests.Add(new Request
            {
                Title = assignee ?? "Unassigned",
                Description = "Scope test",
                CategoryId = category.Id,
                RequesterId = "requester",
                AssigneeId = assignee,
                State = assignee is null ? RequestState.Open : RequestState.Assigned,
                CreatedAt = DateTime.UtcNow
            });
        }
        await db.SaveChangesAsync();
        using var client = _factory.CreateClient();
        if (manager) client.AsManager();
        else client.AsStaff("staff-a");

        var expectedIds = db.Requests
            .Where(r => r.CategoryId == category.Id && (manager || r.AssigneeId == null || r.AssigneeId == "staff-a"))
            .OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id)
            .Select(r => r.Id).ToArray();
        for (var page = 1; page <= expectedIds.Length; page++)
        {
            var result = await client.GetFromJsonAsync<PagedResultDto<StaffRequestListItemDto>>(
                $"/api/staff/requests?categoryId={category.Id}&page={page}&pageSize=1");
            Assert.NotNull(result);
            Assert.Equal(expectedIds.Length, result.TotalCount);
            Assert.Equal(expectedIds.Length, result.TotalPages);
            Assert.Equal(expectedIds[page - 1], Assert.Single(result.Items).Id);
        }

        var filtered = await client.GetFromJsonAsync<PagedResultDto<StaffRequestListItemDto>>(
            $"/api/staff/requests?categoryId={category.Id}&assigneeId=staff-b");
        Assert.NotNull(filtered);
        Assert.Equal(manager ? 1 : 0, filtered.TotalCount);
        Assert.Equal(manager ? 1 : 0, filtered.Items.Count);
    }
}
