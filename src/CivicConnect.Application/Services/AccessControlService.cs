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
        // Admins can access everything
        if (user.IsInRole("Admin"))
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

        return request.RequesterId == userId;
    }
}
