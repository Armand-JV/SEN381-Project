using System;
using System.Collections.Generic;
using CivicConnect.Domain.Enums;

namespace CivicConnect.Application.Policies;

public static class RequestStateTransitionPolicy
{
    private static readonly IReadOnlyDictionary<RequestState, IReadOnlySet<RequestState>> PermittedTransitions =
        new Dictionary<RequestState, IReadOnlySet<RequestState>>
        {
            [RequestState.Open] = new HashSet<RequestState>
            {
                RequestState.Assigned,
                RequestState.Rejected,
                RequestState.Cancelled
            },
            [RequestState.Assigned] = new HashSet<RequestState>
            {
                RequestState.InProgress,
                RequestState.Rejected,
                RequestState.Cancelled
            },
            [RequestState.InProgress] = new HashSet<RequestState>
            {
                RequestState.Resolved,
                RequestState.Cancelled
            },
            [RequestState.Resolved] = new HashSet<RequestState>
            {
                RequestState.Closed
            },
            [RequestState.Closed] = new HashSet<RequestState>(),
            [RequestState.Cancelled] = new HashSet<RequestState>(),
            [RequestState.Rejected] = new HashSet<RequestState>()
        };

    public static bool IsAllowed(RequestState currentState, RequestState nextState) =>
        Enum.IsDefined(currentState) &&
        Enum.IsDefined(nextState) &&
        PermittedTransitions.TryGetValue(currentState, out var transitions) &&
        transitions.Contains(nextState);

    public static bool RequiresReason(RequestState nextState) =>
        nextState is RequestState.Rejected or RequestState.Cancelled;
}
