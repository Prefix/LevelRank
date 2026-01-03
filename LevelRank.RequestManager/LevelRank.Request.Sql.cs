using LevelRank.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using SqlSugar;

namespace LevelRank.Request.Sql;

public class LevelRankRequestManager : IModSharpModule
{
    private readonly SqlRequestManager _sqlRequestManager;

    private readonly ISharedSystem                    _sharedSystem;
    private readonly string                           _dllPath;
    private readonly string                           _sharpPath;
    private readonly ILogger<LevelRankRequestManager> _logger;

    public LevelRankRequestManager(
        ISharedSystem  sharedSystem,
        string         dllPath,
        string         sharpPath,
        Version        version,
        IConfiguration configuration,
        bool           hotReload)
    {
        _sharedSystem = sharedSystem;
        _dllPath      = dllPath;
        _sharpPath    = sharpPath;

        var loggerFactory = _sharedSystem.GetLoggerFactory();
        _logger = loggerFactory.CreateLogger<LevelRankRequestManager>();

        var connectionConfig = BuildConnectionConfig(sharpPath);
        _sqlRequestManager   = new SqlRequestManager(connectionConfig, loggerFactory);
    }

    private ConnectionConfig BuildConnectionConfig(string sharpPath)
    {
        var configuration = LoadConfiguration(sharpPath);

        var dbTypeStr = configuration["Database:Type"]     ?? "MySql";
        var host      = configuration["Database:Host"]     ?? "localhost";
        var port      = configuration["Database:Port"]     ?? "3306";
        var database  = configuration["Database:Database"] ?? "levelrank";
        var user      = configuration["Database:User"]     ?? "root";
        var password  = configuration["Database:Password"] ?? "";

        var dbType = dbTypeStr.ToLowerInvariant() switch
        {
            "mysql"      => DbType.MySql,
            "postgresql" => DbType.PostgreSQL,
            "sqlite"     => DbType.Sqlite,
            _            => throw new NotSupportedException($"Database type '{dbTypeStr}' is not supported. Supported types: mysql, postgresql, sqlite"),
        };

        var connectionString = dbType switch
        {
            DbType.MySql      => $"Server={host};Port={port};Database={database};User={user};Password={password};",
            DbType.PostgreSQL => $"Host={host};Port={port};Database={database};Username={user};Password={password};",
            DbType.Sqlite     => BuildSqliteConnectionString(sharpPath, database),
            _                 => throw new NotSupportedException($"Database type '{dbTypeStr}' is not supported."),
        };

        return new ConnectionConfig
        {
            DbType                = dbType,
            ConnectionString      = connectionString,
            IsAutoCloseConnection = true,
            InitKeyType           = InitKeyType.Attribute,
            MoreSettings = new()
            {
                DisableNvarchar = true,
            },
            LanguageType = LanguageType.English,
        };
    }

    private static IConfigurationRoot LoadConfiguration(string sharpPath)
    {
        var configPath = Path.Combine(Path.GetFullPath(sharpPath), "configs");

        return new ConfigurationBuilder()
               .SetBasePath(configPath)
               .AddJsonFile("levelrank.jsonc", false, false)
               .Build();
    }

    private static string BuildSqliteConnectionString(string sharpPath, string database)
    {
        var dataDir = Path.Combine(sharpPath, "data");
        Directory.CreateDirectory(dataDir);
        var dbPath = Path.Combine(dataDir, $"{database}.db");

        return $"Data Source={dbPath}";
    }

    public bool Init()
    {
        _sqlRequestManager.Init();
        return true;
    }

    public void PostInit()
    {
        _sharedSystem.GetSharpModuleManager().RegisterSharpModuleInterface<IRequestManager>(this, IRequestManager.Identity, _sqlRequestManager);
    }

    public void Shutdown()
    {
        _sqlRequestManager.Dispose();
    }

    string IModSharpModule.DisplayName   => "[LevelRank] RequestManager - SQL";
    string IModSharpModule.DisplayAuthor => "Nukoooo";
}