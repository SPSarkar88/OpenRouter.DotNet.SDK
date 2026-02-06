using System;
using System.IO;
using System.Collections.Generic;

namespace OpenRouter.Examples.EnvConfig;

/// <summary>
/// Shared configuration for all example projects.
/// Reads from .env file in the solution root.
/// </summary>
public static class ExampleConfig
{
    private static readonly Dictionary<string, string> _config = new();
    private static bool _loaded = false;

    /// <summary>
    /// Gets the OpenRouter API key from .env file or environment variable.
    /// </summary>
    public static string ApiKey => GetConfigValue("OPENROUTER_API_KEY");

    /// <summary>
    /// Gets the default model name to use across examples.
    /// </summary>
    public static string ModelName => GetConfigValue("OPENROUTER_MODEL", "openai/gpt-3.5-turbo");

    /// <summary>
    /// Gets the base URL for OpenRouter API.
    /// </summary>
    public static string BaseUrl => GetConfigValue("OPENROUTER_BASE_URL", "https://openrouter.ai/api/v1");

    /// <summary>
    /// Gets the site URL for analytics (optional).
    /// </summary>
    public static string? SiteUrl => GetConfigValue("OPENROUTER_SITE_URL", null);

    /// <summary>
    /// Gets the site name for analytics (optional).
    /// </summary>
    public static string? SiteName => GetConfigValue("OPENROUTER_SITE_NAME", null);

    private static string GetConfigValue(string key, string? defaultValue = null)
    {
        EnsureLoaded();

        // Check config dictionary first (from .env)
        if (_config.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        // Fall back to environment variable
        var envValue = Environment.GetEnvironmentVariable(key);
        if (!string.IsNullOrWhiteSpace(envValue))
        {
            return envValue;
        }

        // Return default or throw
        if (defaultValue != null)
        {
            return defaultValue;
        }

        throw new InvalidOperationException(
            $"Configuration value '{key}' not found. Please set it in .env file or environment variable.");
    }

    private static void EnsureLoaded()
    {
        if (_loaded) return;

        // Find the .env file by walking up the directory tree
        var currentDir = Directory.GetCurrentDirectory();
        var envFile = FindEnvFile(currentDir);

        if (envFile != null)
        {
            LoadEnvFile(envFile);
        }

        _loaded = true;
    }

    private static string? FindEnvFile(string startPath)
    {
        var dir = new DirectoryInfo(startPath);

        while (dir != null)
        {
            var envPath = Path.Combine(dir.FullName, ".env");
            if (File.Exists(envPath))
            {
                return envPath;
            }

            // Also check parent directories
            dir = dir.Parent;

            // Stop at a reasonable level (e.g., when we find a .sln file or reach root)
            if (dir?.Parent == null || File.Exists(Path.Combine(dir.FullName, "*.sln")))
            {
                break;
            }
        }

        return null;
    }

    private static void LoadEnvFile(string filePath)
    {
        try
        {
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                // Skip empty lines and comments
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                {
                    continue;
                }

                // Parse KEY=VALUE format
                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    // Remove quotes if present
                    if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                        (value.StartsWith("'") && value.EndsWith("'")))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    _config[key] = value;
                }
            }

            Console.WriteLine($"Loaded configuration from: {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not load .env file: {ex.Message}");
        }
    }

    /// <summary>
    /// Prints the current configuration (with masked API key).
    /// </summary>
    public static void PrintConfig()
    {
        EnsureLoaded();

        Console.WriteLine("Configuration:");
        Console.WriteLine($"  API Key: {MaskApiKey(ApiKey)}");
        Console.WriteLine($"  Model: {ModelName}");
        Console.WriteLine($"  Base URL: {BaseUrl}");
        
        if (!string.IsNullOrEmpty(SiteUrl))
        {
            Console.WriteLine($"  Site URL: {SiteUrl}");
        }
        
        if (!string.IsNullOrEmpty(SiteName))
        {
            Console.WriteLine($"  Site Name: {SiteName}");
        }
    }

    private static string MaskApiKey(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey.Length < 20)
        {
            return "***";
        }

        return apiKey.Substring(0, 15) + "...";
    }
}
