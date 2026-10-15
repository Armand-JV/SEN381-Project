using Microsoft.AspNetCore.Authorization;
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
        if (user.IsInRole("Admin") || user.IsInRole("MANAGER"))
        {
            return true;
        }

        // Check if the current user is the owner of the request
        // Using NameIdentifier (ClaimTypes.NameIdentifier) which is the standard JWT "sub"
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Fallback to Name if NameIdentifier is null
        if (string.IsNullOrEmpty(userId))
        {
            userId = user.Identity?.Name;
        }

        if (user.IsInRole("STAFF"))
        {
            // Staff see unassigned work and their own assignments only.
            return request.AssigneeId is null || request.AssigneeId == userId;
        }

        return request.RequesterId == userId;
    }
}
