using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CivicConnect.Tests;

/// <summary>
/// Supplies role and user headers consumed by the test-only authentication handler.
/// </summary>
public static class TestAuthenticationHelper
{
    public const string RoleHeaderName = "X-Test-Role";
    public const string UserHeaderName = "X-Test-User";

    public enum TestRole
    {
        Anonymous,
        Requester,
        Staff,
        Manager
    }

    /// <summary>
    /// Stamps the test role and user ID onto <paramref name="client"/>.
    /// </summary>
    public static HttpClient WithRole(this HttpClient client, TestRole role, string? userId = null)
    {
        client.DefaultRequestHeaders.Remove(RoleHeaderName);
        client.DefaultRequestHeaders.Remove(UserHeaderName);

        if (role != TestRole.Anonymous)
        {
            client.DefaultRequestHeaders.Add(RoleHeaderName, role.ToString().ToUpperInvariant());
        }

        if (!string.IsNullOrEmpty(userId))
        {
            client.DefaultRequestHeaders.Add(UserHeaderName, userId);
        }

        return client;
    }

    public static HttpClient AsRequester(this HttpClient client, string? userId = null) =>
        client.WithRole(TestRole.Requester, userId);

    public static HttpClient AsStaff(this HttpClient client, string? userId = null) =>
        client.WithRole(TestRole.Staff, userId);

    public static HttpClient AsManager(this HttpClient client, string? userId = null) =>
        client.WithRole(TestRole.Manager, userId);

    public static HttpClient AsAnonymous(this HttpClient client) =>
        client.WithRole(TestRole.Anonymous);
}