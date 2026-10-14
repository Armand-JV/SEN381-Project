using System;
using System.Threading.Tasks;
using CivicConnect.Application.Patterns.Observer;
using CivicConnect.Domain.Entities;
using CivicConnect.Domain.Enums;
using CivicConnect.Tests.Builders;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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

        var notifier = new RequestStateNotifier(
            new[] { observer1.Object, observer2.Object },
            NullLogger<RequestStateNotifier>.Instance);

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
        var notifier = new RequestStateNotifier(
            Array.Empty<IRequestStateObserver>(),
            NullLogger<RequestStateNotifier>.Instance);

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

        var notifier = new RequestStateNotifier(
            new[] { observer.Object },
            NullLogger<RequestStateNotifier>.Instance);

        // Act
        await notifier.NotifyStateChangedAsync(request, RequestState.InProgress, RequestState.Resolved);

        // Assert
        capturedOld.Should().Be(RequestState.InProgress);
        capturedNew.Should().Be(RequestState.Resolved);
    }

    [Fact]
    public async Task NotifyStateChangedAsync_WhenObserverThrows_LogsAndContinues()
    {
        var request = new RequestBuilder().Build();
        var failingObserver = new Mock<IRequestStateObserver>();
        var succeedingObserver = new Mock<IRequestStateObserver>();
        var logger = new Mock<ILogger<RequestStateNotifier>>();
        logger.Setup(log => log.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        failingObserver
            .Setup(observer => observer.OnStateChangedAsync(request, RequestState.Open, RequestState.Assigned))
            .ThrowsAsync(new InvalidOperationException("Observer failure"));

        var notifier = new RequestStateNotifier(
            new[] { failingObserver.Object, succeedingObserver.Object },
            logger.Object);

        await notifier.NotifyStateChangedAsync(request, RequestState.Open, RequestState.Assigned);

        failingObserver.Verify(observer => observer.OnStateChangedAsync(request, RequestState.Open, RequestState.Assigned), Times.Once);
        succeedingObserver.Verify(observer => observer.OnStateChangedAsync(request, RequestState.Open, RequestState.Assigned), Times.Once);
        VerifyLog(logger, LogLevel.Error, "failed for request");
    }

    [Fact]
    public async Task LogRequestStateObserver_LogsStateChange()
    {
        var request = new RequestBuilder().WithRequesterId("requester-7").Build();
        request.Id = 42;
        var logger = new Mock<ILogger<LogRequestStateObserver>>();
        logger.Setup(log => log.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var observer = new LogRequestStateObserver(logger.Object);

        await observer.OnStateChangedAsync(request, RequestState.Assigned, RequestState.InProgress);

        VerifyLog(logger, LogLevel.Information, "Request 42 changed state from Assigned to InProgress");
        VerifyLog(logger, LogLevel.Information, "requester-7");
    }

    private static void VerifyLog<T>(Mock<ILogger<T>> logger, LogLevel level, string messagePart)
    {
#pragma warning disable CA1873
        logger.Verify(
            log => log.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((value, _) => value!.ToString()!.Contains(messagePart)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
#pragma warning restore CA1873
    }
}
