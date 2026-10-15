using TransitionMap = CivicConnect.Application.Policies.RequestStateTransitionPolicy;
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

    [Fact]
    public void IsAllowed_ClosedToOpen_ReturnsFalse()
    {
        TransitionMap.IsAllowed(RequestState.Closed, RequestState.Open).Should().BeFalse();
    }
}
