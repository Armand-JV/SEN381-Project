using TransitionMap = CivicConnect.Application.Services.RequestStateTransitionPolicy;
using CivicConnect.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CivicConnect.Tests;

/// <summary>
/// TC-U-03 (Issue #48): unit verification of the transition table.
/// TC-U-04 role policy has no unit entry point — enforced by [Authorize(Roles)] on the
/// controller, verified through HTTP in RequestControllerTests.
/// </summary>
public class RequestTransitionPolicyTests
{
    [Fact]
    public void IsAllowed_OpenToAssigned_ReturnsTrue()
    {
        TransitionMap.IsAllowed(RequestState.Open, RequestState.Assigned).Should().BeTrue();
    }

    [Theory]
    [InlineData(RequestState.Open, RequestState.Assigned, true)]
    [InlineData(RequestState.Open, RequestState.Rejected, true)]
    [InlineData(RequestState.Open, RequestState.Cancelled, true)]
    [InlineData(RequestState.Assigned, RequestState.InProgress, true)]
    [InlineData(RequestState.Assigned, RequestState.Rejected, true)]
    [InlineData(RequestState.Assigned, RequestState.Cancelled, true)]
    [InlineData(RequestState.InProgress, RequestState.Resolved, true)]
    [InlineData(RequestState.InProgress, RequestState.Cancelled, true)]
    [InlineData(RequestState.Resolved, RequestState.Closed, true)]
    [InlineData(RequestState.InProgress, RequestState.Rejected, false)]
    [InlineData(RequestState.InProgress, RequestState.Closed, false)]
    [InlineData(RequestState.Resolved, RequestState.Cancelled, false)]
    [InlineData(RequestState.Closed, RequestState.Open, false)]
    [InlineData(RequestState.Cancelled, RequestState.Open, false)]
    [InlineData(RequestState.Rejected, RequestState.Open, false)]
    public void IsAllowed_ReturnsExpectedResult(RequestState current, RequestState next, bool expected)
    {
        TransitionMap.IsAllowed(current, next).Should().Be(expected);
    }
}
