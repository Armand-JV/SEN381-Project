using System.Threading.Tasks;
using CivicConnect.Application.Patterns.Observer;
using CivicConnect.Domain.Entities;
using CivicConnect.Domain.Enums;
using CivicConnect.Tests.Builders;
using FluentAssertions;
using Moq;
using Xunit;

namespace CivicConnect.Tests;

/// <summary>
/// Unit tests for <see cref="RequestStateNotifier"/>, verifying the Observer pattern
/// correctly fans out state-change notifications to all registered observers.
/// </summary>
public class RequestStateNotifierTests
{
    [Fact]
    public async Task NotifyStateChangedAsync_CallsAllObservers()
    {
        // Arrange
        var request = new RequestBuilder().WithState(RequestState.Assigned).Build();
        var oldState = RequestState.Open;
        var newState = RequestState.Assigned;

        var observer1 = new Mock<IRequestStateObserver>();
        var observer2 = new Mock<IRequestStateObserver>();

        var notifier = new RequestStateNotifier(new[] { observer1.Object, observer2.Object });

        // Act
        await notifier.NotifyStateChangedAsync(request, oldState, newState);

        // Assert
        observer1.Verify(o => o.OnStateChangedAsync(request, oldState, newState), Times.Once);
        observer2.Verify(o => o.OnStateChangedAsync(request, oldState, newState), Times.Once);
    }

    [Fact]
    public async Task NotifyStateChangedAsync_WithNoObservers_DoesNotThrow()
    {
        // Arrange
        var request = new RequestBuilder().Build();
        var notifier = new RequestStateNotifier(System.Array.Empty<IRequestStateObserver>());

        // Act & Assert – should complete without error
        await notifier.NotifyStateChangedAsync(request, RequestState.Open, RequestState.InProgress);
    }

    [Fact]
    public async Task NotifyStateChangedAsync_PassesCorrectStatesToObserver()
    {
        // Arrange
        var request = new RequestBuilder().WithState(RequestState.Resolved).Build();
        RequestState? capturedOld = null;
        RequestState? capturedNew = null;

        var observer = new Mock<IRequestStateObserver>();
        observer.Setup(o => o.OnStateChangedAsync(It.IsAny<Request>(), It.IsAny<RequestState>(), It.IsAny<RequestState>()))
            .Callback<Request, RequestState, RequestState>((_, old, @new) =>
            {
                capturedOld = old;
                capturedNew = @new;
            })
            .Returns(Task.CompletedTask);

        var notifier = new RequestStateNotifier(new[] { observer.Object });

        // Act
        await notifier.NotifyStateChangedAsync(request, RequestState.InProgress, RequestState.Resolved);

        // Assert
        capturedOld.Should().Be(RequestState.InProgress);
        capturedNew.Should().Be(RequestState.Resolved);
    }
}
