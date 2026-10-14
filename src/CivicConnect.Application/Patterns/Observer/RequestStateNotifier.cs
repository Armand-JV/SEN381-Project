using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CivicConnect.Domain.Entities;
using CivicConnect.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CivicConnect.Application.Patterns.Observer;

public class RequestStateNotifier
{
    private readonly IEnumerable<IRequestStateObserver> _observers;
    private readonly ILogger<RequestStateNotifier> _logger;

    public RequestStateNotifier(
        IEnumerable<IRequestStateObserver> observers,
        ILogger<RequestStateNotifier> logger)
    {
        _observers = observers;
        _logger = logger;
    }

    public async Task NotifyStateChangedAsync(Request request, RequestState oldState, RequestState newState)
    {
        foreach (var observer in _observers)
        {
            try
            {
                await observer.OnStateChangedAsync(request, oldState, newState);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Request state observer {ObserverType} failed for request {RequestId} changing from {OldState} to {NewState}.",
                    observer.GetType().Name, request.Id, oldState, newState);
            }
        }
    }
}
