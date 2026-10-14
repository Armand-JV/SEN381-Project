using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CivicConnect.Web.Configuration;

public static class EnvLoader
{
    /// <summary>
    /// Searches candidate paths and loads the first found .env file into environment variables.
    /// Supports both 'KEY: VALUE' and 'KEY=VALUE' formats.
    /// </summary>
    public static void Load(string? basePath = null)
    {
        var candidates = new List<string>();

        if (!string.IsNullOrEmpty(basePath))
        {
            candidates.Add(Path.Combine(basePath, ".env"));
        }

        candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
        candidates.Add(Path.Combine(AppContext.BaseDirectory, ".env"));

        // Walk up directory tree to locate .env in project or solution root
        var currentDir = new DirectoryInfo(basePath ?? Directory.GetCurrentDirectory());
        while (currentDir != null)
        {
            var envPath = Path.Combine(currentDir.FullName, ".env");
            if (!candidates.Contains(envPath))
            {
                candidates.Add(envPath);
            }

            if (Directory.Exists(Path.Combine(currentDir.FullName, ".git")))
            {
                break;
            }

            currentDir = currentDir.Parent;
        }

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                LoadFile(candidate);
                break;
            }
        }
    }

    /// <summary>
    /// Parses a .env file and sets environment variables if they are not already set.
    /// </summary>
    public static void LoadFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        foreach (var rawLine in File.ReadAllLines(filePath))
        {
            var line = rawLine.Trim();

            // Ignore empty lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            // Support either 'KEY: VALUE' or 'KEY=VALUE'
            var separatorIndex = line.AsSpan().IndexOfAny(':', '=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line.Substring(0, separatorIndex).Trim();
            var value = line.Substring(separatorIndex + 1).Trim();

            // Strip optional surrounding double or single quotes
            if ((value.StartsWith('"') && value.EndsWith('"')) ||
                (value.StartsWith('\'') && value.EndsWith('\'')))
            {
                if (value.Length >= 2)
                {
                    value = value.Substring(1, value.Length - 2);
                }
            }

            // Set environment variable if unset or empty in the process
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    /// <summary>
    /// Builds the PostgreSQL connection string using individual database environment variables,
    /// falling back to the configured connection string or default development connection.
    /// </summary>
    public static string GetConnectionString(IConfiguration configuration)
    {
        var dbIp = Environment.GetEnvironmentVariable("DB_IP")
                   ?? configuration["DB_IP"]
                   ?? Environment.GetEnvironmentVariable("DB_HOST")
                   ?? configuration["DB_HOST"];

        var dbName = Environment.GetEnvironmentVariable("DB_NAME")
                     ?? configuration["DB_NAME"]
                     ?? Environment.GetEnvironmentVariable("DB_DATABASE")
                     ?? configuration["DB_DATABASE"];

        if (!string.IsNullOrWhiteSpace(dbIp) && !string.IsNullOrWhiteSpace(dbName))
        {
            var dbUser = Environment.GetEnvironmentVariable("DB_USER")
                         ?? configuration["DB_USER"]
                         ?? Environment.GetEnvironmentVariable("DB_USERNAME")
                         ?? configuration["DB_USERNAME"];

            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD")
                             ?? configuration["DB_PASSWORD"];

            var dbPort = Environment.GetEnvironmentVariable("DB_PORT")
                         ?? configuration["DB_PORT"];

            var dbSslMode = Environment.GetEnvironmentVariable("DB_SSLMODE")
                            ?? configuration["DB_SSLMODE"];

            var csBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = dbIp,
                Database = dbName,
                Username = dbUser ?? string.Empty
            };

            if (!string.IsNullOrEmpty(dbPassword))
            {
                csBuilder.Password = dbPassword;
            }

            if (int.TryParse(dbPort, out var port))
            {
                csBuilder.Port = port;
            }

            if (!string.IsNullOrWhiteSpace(dbSslMode) &&
                Enum.TryParse<SslMode>(dbSslMode, ignoreCase: true, out var sslMode))
            {
                csBuilder.SslMode = sslMode;
            }

            return csBuilder.ConnectionString;
        }

        var defaultConn = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(defaultConn))
        {
            return defaultConn;
        }

        return "Host=localhost;Database=CivicConnect;";
    }
}
