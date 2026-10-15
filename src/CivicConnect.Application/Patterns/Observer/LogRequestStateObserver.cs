using System.Threading.Tasks;
using CivicConnect.Domain.Entities;
using CivicConnect.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CivicConnect.Application.Patterns.Observer;

public partial class LogRequestStateObserver : IRequestStateObserver
{
    private readonly ILogger<LogRequestStateObserver> _logger;

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Request {RequestId} changed state from {OldState} to {NewState} (Requester: {RequesterId}).")]
    private static partial void LogStateChanged(
        ILogger logger,
        int requestId,
        RequestState oldState,
        RequestState newState,
        string requesterId);

    public LogRequestStateObserver(ILogger<LogRequestStateObserver> logger)
    {
        _logger = logger;
    }

    public Task OnStateChangedAsync(Request request, RequestState oldState, RequestState newState)
    {
        // Note: log-based observer is best-effort only; a durable notification/outbox
        // with retry semantics can replace this when guaranteed delivery is required.
        LogStateChanged(_logger, request.Id, oldState, newState, request.RequesterId);

        return Task.CompletedTask;
    }
}
