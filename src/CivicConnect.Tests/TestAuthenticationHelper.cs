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
/// Test-authentication helper.
///
/// The production application has no auth middleware wired up yet (NFR-017 is a future
/// work item), so this helper does not forge real tokens. Instead it exposes the three
/// auth roles the codebase already models (<see cref="TestRole"/>) and provides:
///   - <see cref="WithRole"/> to stamp a role onto an <see cref="HttpClient"/> via a
///     request header the application can read when auth is implemented.
///   - <see cref="AsRequester"/>, <see cref="AsStaff"/>, <see cref="AsManager"/> as the
///     ready-made role presets used by the black-box and negative-behaviour suites.
///
/// When the team lands real authentication this helper is the single place to update.
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
    /// Stamps the test role and user id onto <paramref name="client"/>. Callers can also
    /// supply a raw <see cref="AuthenticationScheme"/> (e.g. "Bearer") for future use.
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