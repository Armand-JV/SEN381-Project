using System.Threading.Tasks;
using CivicConnect.Domain.Entities;
using CivicConnect.Domain.Enums;

namespace CivicConnect.Application.Patterns.Observer;

public class LogRequestStateObserver : IRequestStateObserver
{
    public async Task OnStateChangedAsync(Request request, RequestState oldState, RequestState newState)
    {
        // Placeholder for logging implementation
        await Task.CompletedTask;
    }
}
