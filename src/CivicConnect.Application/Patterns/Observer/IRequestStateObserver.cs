using System.Threading.Tasks;
using CivicConnect.Domain.Entities;
using CivicConnect.Domain.Enums;

namespace CivicConnect.Application.Patterns.Observer;

public interface IRequestStateObserver
{
    Task OnStateChangedAsync(Request request, RequestState oldState, RequestState newState);
}
