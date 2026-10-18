using System.Security.Claims;
using CivicConnect.Domain.Entities;

namespace CivicConnect.Application.Services;

public interface IAccessControlService
{
    bool CanAccess(ClaimsPrincipal user, Request request);
}

public class AccessControlService : IAccessControlService
{
    public bool CanAccess(ClaimsPrincipal user, Request request)
    {
        // Admins and managers can access everything
        if (user.IsInRole("ADMIN") || user.IsInRole("MANAGER"))
        {
            return true;
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.Identity?.Name;

        if (user.IsInRole("STAFF"))
        {
            // Staff see unassigned work and their own assignments only.
            return request.AssigneeId is null || request.AssigneeId == userId;
        }

        return request.RequesterId == userId;
    }
}
