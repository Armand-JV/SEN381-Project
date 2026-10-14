using System.Threading.Tasks;
using CivicConnect.Domain.Entities;
using CivicConnect.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CivicConnect.Application.Patterns.Observer;

public class LogRequestStateObserver : IRequestStateObserver
{
    private readonly ILogger<LogRequestStateObserver> _logger;

    public LogRequestStateObserver(ILogger<LogRequestStateObserver> logger)
    {
        _logger = logger;
    }

    public Task OnStateChangedAsync(Request request, RequestState oldState, RequestState newState)
    {
        // ponytail: log-based observer is best-effort only; a durable notification/outbox
        // with retry semantics can replace this when guaranteed delivery is required.
        _logger.LogInformation(
            "Request {RequestId} changed state from {OldState} to {NewState} (Requester: {RequesterId}).",
            request.Id, oldState, newState, request.RequesterId);

        return Task.CompletedTask;
    }
}
