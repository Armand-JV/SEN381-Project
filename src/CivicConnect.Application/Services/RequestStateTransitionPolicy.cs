using System.Security.Claims;
using CivicConnect.Domain.Entities;
using CivicConnect.Domain.Enums;

namespace CivicConnect.Application.Services;

public sealed class RequestStateTransitionPolicy
{
    public bool TryValidate(Request request, RequestState newState, ClaimsPrincipal user, string? reason, out string error)
    {
        if (IsTerminal(request.State))
        {
            error = $"Cannot transition from terminal state {request.State}.";
            return false;
        }

        if (!IsAllowed(request.State, newState))
        {
            error = $"Cannot transition from {request.State} to {newState}.";
            return false;
        }

        if (IsTerminal(newState) && string.IsNullOrWhiteSpace(reason))
        {
            error = $"A reason is required when {newState.ToString().ToLowerInvariant()} a request.";
            return false;
        }

        if ((newState is RequestState.Rejected or RequestState.Closed) && !CanManageRequest(user))
        {
            error = $"Only staff or managers may set a request to {newState}.";
            return false;
        }

        if (newState == RequestState.Cancelled && !CanCancel(request, user))
        {
            error = "Only the requester, staff, or managers may cancel a request.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static bool IsAllowed(RequestState from, RequestState to) => (from, to) switch
    {
        (RequestState.Open, RequestState.Assigned or RequestState.Rejected or RequestState.Cancelled) => true,
        (RequestState.Assigned, RequestState.InProgress or RequestState.Rejected or RequestState.Cancelled) => true,
        (RequestState.InProgress, RequestState.Resolved or RequestState.Rejected or RequestState.Cancelled or RequestState.Closed) => true,
        (RequestState.Resolved, RequestState.Closed) => true,
        _ => false
    };

    private static bool IsTerminal(RequestState state) =>
        state is RequestState.Closed or RequestState.Cancelled or RequestState.Rejected;

    private static bool CanCancel(Request request, ClaimsPrincipal user) =>
        CanManageRequest(user) || request.RequesterId == GetUserId(user);

    private static bool CanManageRequest(ClaimsPrincipal user) =>
        user.IsInRole("Admin") || user.IsInRole("Manager") || user.IsInRole("Staff");

    private static string? GetUserId(ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.Identity?.Name;
}
