using System;
using CivicConnect.Domain.Entities;
using CivicConnect.Domain.Enums;
using CivicConnect.Tests.Builders;
using FluentAssertions;
using Xunit;

namespace CivicConnect.Tests;

/// <summary>
/// Unit tests for domain entities: <see cref="Request"/>, <see cref="Category"/>,
/// <see cref="StatusHistory"/>, and <see cref="Comment"/>. Verifies default values,
/// builder correctness, and entity relationships.
/// </summary>
public class DomainEntityTests
{
    [Fact]
    public void Request_DefaultState_IsOpen()
    {
        var request = new Request();
        request.State.Should().Be(RequestState.Open);
    }

    [Fact]
    public void RequestBuilder_ProducesValidDefaults()
    {
        var request = new RequestBuilder().Build();

        request.Title.Should().NotBeNullOrWhiteSpace();
        request.Description.Should().NotBeNullOrWhiteSpace();
        request.CategoryId.Should().BeGreaterThan(0);
        request.RequesterId.Should().NotBeNullOrWhiteSpace();
        request.State.Should().Be(RequestState.Open);
        request.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void RequestBuilder_OverridesAllProperties()
    {
        var created = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var request = new RequestBuilder()
            .WithTitle("Custom Title")
            .WithDescription("Custom Description")
            .WithCategoryId(3)
            .WithState(RequestState.Resolved)
            .WithRequesterId("admin42")
            .WithCreatedAt(created)
            .Build();

        request.Title.Should().Be("Custom Title");
        request.Description.Should().Be("Custom Description");
        request.CategoryId.Should().Be(3);
        request.State.Should().Be(RequestState.Resolved);
        request.RequesterId.Should().Be("admin42");
        request.CreatedAt.Should().Be(created);
    }

    [Fact]
    public void CategoryBuilder_ProducesValidDefaults()
    {
        var category = new CategoryBuilder().Build();

        category.Id.Should().Be(1);
        category.Name.Should().Be("Infrastructure");
        category.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void CategoryBuilder_OverridesAllProperties()
    {
        var category = new CategoryBuilder()
            .WithId(10)
            .WithName("Custom")
            .WithDescription("Custom desc")
            .Build();

        category.Id.Should().Be(10);
        category.Name.Should().Be("Custom");
        category.Description.Should().Be("Custom desc");
    }

    [Fact]
    public void Request_CommentsCollection_IsInitializedEmpty()
    {
        var request = new Request();
        request.Comments.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Request_HistoryCollection_IsInitializedEmpty()
    {
        var request = new Request();
        request.History.Should().NotBeNull().And.BeEmpty();
    }

    [Theory]
    [InlineData(RequestState.Open)]
    [InlineData(RequestState.Assigned)]
    [InlineData(RequestState.InProgress)]
    [InlineData(RequestState.Resolved)]
    [InlineData(RequestState.Closed)]
    [InlineData(RequestState.Cancelled)]
    [InlineData(RequestState.Rejected)]
    public void RequestState_AllValues_AreAccessible(RequestState state)
    {
        var request = new RequestBuilder().WithState(state).Build();
        request.State.Should().Be(state);
    }

    [Fact]
    public void StatusHistory_CanRecordTransition()
    {
        var history = new StatusHistory
        {
            RequestId = 1,
            PreviousState = RequestState.Open,
            NewState = RequestState.Assigned,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = "staff1",
            Reason = "Assigned to team"
        };

        history.PreviousState.Should().Be(RequestState.Open);
        history.NewState.Should().Be(RequestState.Assigned);
        history.ChangedBy.Should().Be("staff1");
        history.Reason.Should().Be("Assigned to team");
    }

    [Fact]
    public void Comment_CanBeCreated()
    {
        var comment = new Comment
        {
            RequestId = 1,
            Text = "Work in progress",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "staff1",
            IsInternal = true
        };

        comment.Text.Should().Be("Work in progress");
        comment.IsInternal.Should().BeTrue();
        comment.CreatedBy.Should().Be("staff1");
    }
}
