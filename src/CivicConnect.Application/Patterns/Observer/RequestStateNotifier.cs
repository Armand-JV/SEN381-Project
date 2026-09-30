using System.Collections.Generic;
using System.Threading.Tasks;
using CivicConnect.Domain.Entities;
using CivicConnect.Domain.Enums;

namespace CivicConnect.Application.Patterns.Observer;

public class RequestStateNotifier
{
    private readonly IEnumerable<IRequestStateObserver> _observers;

    public RequestStateNotifier(IEnumerable<IRequestStateObserver> observers)
    {
        _observers = observers;
    }

    public async Task NotifyStateChangedAsync(Request request, RequestState oldState, RequestState newState)
    {
        foreach (var observer in _observers)
        {
            await observer.OnStateChangedAsync(request, oldState, newState);
        }
    }
}
