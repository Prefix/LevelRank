using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sharp.Shared.Objects;

namespace LevelRank.Managers;

internal interface IConfigManager : IManager
{
    IConVar CreateConVar(string name, int defaultValue, string description);

    IConVar CreateConVar(string name, string defaultValue, string description);

    IConVar CreateConVar(string name, bool defaultValue, string description);

    IConVar CreateConVar(string name, float defaultValue, string description);

    void SyncAllConVars();

    void SaveAllConVars();
}

internal class ConfigManager : IConfigManager
{
    private readonly InterfaceBridge        _bridge;
    private readonly ILogger<ConfigManager> _logger;
    private readonly string                 _configPath;
    private readonly List<IConVar>          _registeredConVars = [];

    public ConfigManager(InterfaceBridge bridge, ILogger<ConfigManager> logger)
    {
        _bridge     = bridge;
        _logger     = logger;
        _configPath = Path.Combine(Path.GetFullPath(bridge.SharpPath), "configs", "levelrank.cfg");
    }

    public bool Init() => true;

    public void OnPostInit(ServiceProvider provider)
    {
        SyncAllConVars();
    }

    public void Shutdown()
    {
        SaveAllConVars();
    }

    public IConVar CreateConVar(string name, int defaultValue, string description)
    {
        var cvar = _bridge.ConVarManager.CreateConVar(name, defaultValue, description)
                   ?? throw new InvalidOperationException($"Failed to create ConVar: {name}");

        _registeredConVars.Add(cvar);

        return cvar;
    }

    public IConVar CreateConVar(string name, string defaultValue, string description)
    {
        var cvar = _bridge.ConVarManager.CreateConVar(name, defaultValue, description)
                   ?? throw new InvalidOperationException($"Failed to create ConVar: {name}");

        _registeredConVars.Add(cvar);

        return cvar;
    }

    public IConVar CreateConVar(string name, bool defaultValue, string description)
    {
        var cvar = _bridge.ConVarManager.CreateConVar(name, defaultValue, description)
                   ?? throw new InvalidOperationException($"Failed to create ConVar: {name}");

        _registeredConVars.Add(cvar);

        return cvar;
    }

    public IConVar CreateConVar(string name, float defaultValue, string description)
    {
        var cvar = _bridge.ConVarManager.CreateConVar(name, defaultValue, description)
                   ?? throw new InvalidOperationException($"Failed to create ConVar: {name}");

        _registeredConVars.Add(cvar);

        return cvar;
    }

    public void SyncAllConVars()
        => SyncConVars(_registeredConVars);

    public void SaveAllConVars()
        => SaveConVars(_registeredConVars);

    private void SyncConVars(IReadOnlyList<IConVar> convars)
    {
        var configValues = LoadConfigFile();

        if (configValues.Count == 0 && !File.Exists(_configPath))
        {
            WriteConfigFile(convars);

            return;
        }

        // Apply config values to convars
        foreach (var convar in convars)
        {
            if (configValues.TryGetValue(convar.Name, out var value))
            {
                convar.SetString(value);
            }
        }

        // Check for missing convars and append them
        var missingConvars = convars
                             .Where(c => !configValues.ContainsKey(c.Name))
                             .OrderBy(c => c.Name)
                             .ToList();

        if (missingConvars.Count > 0)
        {
            AppendMissingConVars(missingConvars);
        }
    }

    private void SaveConVars(IReadOnlyList<IConVar> convars)
    {
        WriteConfigFile(convars);
    }

    private Dictionary<string, string> LoadConfigFile()
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!File.Exists(_configPath))
            return values;

        try
        {
            var lines = File.ReadAllLines(_configPath);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var trimmed = line.Trim();

                if (trimmed.StartsWith("//"))
                {
                    continue;
                }

                var parts = trimmed.Split([' ', '\t'], 2, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 2)
                {
                    continue; // Skip lines that don't have a value
                }

                var key   = parts[0];
                var value = parts[1];

                if (value.Length >= 2 && value.StartsWith('"') && value.EndsWith('"'))
                {
                    value = value[1..^1];
                }

                values[key] = value;
            }

            _logger.LogInformation("Loaded {Count} values from {Path}", values.Count, _configPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load config from {Path}", _configPath);
        }

        return values;
    }

    private void WriteConfigFile(IReadOnlyList<IConVar> convars)
    {
        try
        {
            EnsureDirectoryExists();

            var sb = new StringBuilder();
            sb.AppendLine("// LevelRank Configuration");
            sb.AppendLine("// Auto-generated - edit values as needed");
            sb.AppendLine();

            foreach (var convar in convars)
            {
                sb.AppendLine($"// {convar.HelpString}");
                sb.AppendLine($"{convar.Name} {convar.GetString()}");
                sb.AppendLine();
            }

            File.WriteAllText(_configPath, sb.ToString());
            _logger.LogInformation("Saved config to {Path}", _configPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save config to {Path}", _configPath);
        }
    }

    private void AppendMissingConVars(IReadOnlyList<IConVar> convars)
    {
        try
        {
            var sb = new StringBuilder();

            // Ensure we start on a new line if the file doesn't end with one
            var existingContent = File.ReadAllText(_configPath);

            if (existingContent.Length > 0 && !existingContent.EndsWith(Environment.NewLine))
            {
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine("// --- New Settings Added automatically ---");
            sb.AppendLine();

            foreach (var convar in convars)
            {
                AppendConVarBlock(sb, convar);
            }

            File.AppendAllText(_configPath, sb.ToString());
            _logger.LogInformation("Added {Count} new convars to config", convars.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to append convars to {Path}", _configPath);
        }
    }

    private static void AppendConVarBlock(StringBuilder sb, IConVar convar)
    {
        if (!string.IsNullOrEmpty(convar.HelpString))
        {
            sb.AppendLine($"// {convar.HelpString}");
        }

        var rawValue       = convar.GetString();
        var formattedValue = FormatValue(rawValue);

        sb.AppendLine($"{convar.Name} {formattedValue}");
        sb.AppendLine();
    }

    private static string FormatValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "\"\"";
        }

        if (value.Any(char.IsWhiteSpace) && !value.StartsWith('"'))
        {
            return $"\"{value}\"";
        }

        return value;
    }

    private void EnsureDirectoryExists()
    {
        var dir = Path.GetDirectoryName(_configPath);

        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
}
