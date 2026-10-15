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
        if (user.IsInRole("ADMIN") || user.IsInRole("MANAGER"))
        {
            return true;
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.Identity?.Name;

        if (request.RequesterId == userId)
        {
            return true;
        }

        return user.IsInRole("STAFF") && request.AssigneeId == userId;
    }
}
