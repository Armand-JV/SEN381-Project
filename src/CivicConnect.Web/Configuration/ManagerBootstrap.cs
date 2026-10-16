using CivicConnect.Application.Services;

namespace CivicConnect.Web.Configuration;

public static class ManagerBootstrap
{
    public static async Task RunAsync(
        AuthService authService,
        string? username,
        string? email,
        string? password)
    {
        var configuredValues = new[] { username, email, password };
        if (configuredValues.All(string.IsNullOrWhiteSpace))
        {
            return;
        }

        if (configuredValues.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException(
                "BOOTSTRAP_MANAGER_USERNAME, BOOTSTRAP_MANAGER_EMAIL, and BOOTSTRAP_MANAGER_PASSWORD must be configured together.");
        }

        await authService.BootstrapManagerAsync(new ProvisionDto
        {
            Username = username!,
            Email = email!,
            Password = password!,
            Role = "Manager"
        });
    }
}
