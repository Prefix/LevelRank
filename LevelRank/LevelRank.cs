using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sharp.Extensions.GameEventManager;
using Sharp.Shared;
using Sharp.Shared.Abstractions;

namespace LevelRank;

public class LevelRank : IModSharpModule
{
    private readonly ISharedSystem      _shared;
    private readonly ServiceProvider    _serviceProvider;
    private readonly ILogger<LevelRank> _logger;
    private readonly InterfaceBridge    _bridge;

    public LevelRank(
        ISharedSystem  sharedSystem,
        string         dllPath,
        string         sharpPath,
        Version        version,
        IConfiguration configuration,
        bool           hotReload)
    {
        _shared = sharedSystem;
        var loggerFactory = sharedSystem.GetLoggerFactory();
        _logger = loggerFactory.CreateLogger<LevelRank>();

        var bridge = new InterfaceBridge(dllPath,
                                         sharpPath,
                                         version,
                                         sharedSystem,
                                         hotReload,
                                         sharedSystem.GetModSharp()
                                                     .HasCommandLine("-debug"));

        var services = new ServiceCollection();

        services.AddSingleton(bridge);
        services.AddSingleton(loggerFactory);
        services.AddSingleton(sharedSystem);
        services.AddSingleton<IConfiguration>(LoadConfiguration(sharpPath));
        services.AddLogging();
        services.AddGameEventManager();

        services.AddManagerDi();
        services.AddModuleDi();

        _bridge = bridge;

        _serviceProvider = services.BuildServiceProvider();
    }

    public bool Init()
    {
        _serviceProvider.LoadAllSharpExtensions();

        foreach (var service in _serviceProvider.GetServices<IManager>())
        {
            try
            {
                if (service.Init())
                {
                    if (_bridge.Debug)
                    {
                        _logger.LogInformation("{service} Initialized", service.GetType().FullName);
                    }

                    continue;
                }

                _logger.LogError("Failed to init {service}!", service.GetType().FullName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to init {service}!", service.GetType().FullName);
            }

            return false;
        }

        foreach (var service in _serviceProvider.GetServices<IModule>())
        {
            try
            {
                if (service.Init())
                {
                    if (_bridge.Debug)
                    {
                        _logger.LogInformation("{service} Initialized", service.GetType().FullName);
                    }

                    continue;
                }

                _logger.LogError("Failed to init {service}!", service.GetType().FullName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to init {service}!", service.GetType().FullName);
            }

            return false;
        }

        return true;
    }

    public void PostInit()
    {
        foreach (var service in _serviceProvider.GetServices<IManager>())
        {
            try
            {
                service.OnPostInit(_serviceProvider);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when calling PostInit for {service}", service.GetType().FullName);
            }
        }

        foreach (var service in _serviceProvider.GetServices<IModule>())
        {
            try
            {
                service.OnPostInit(_serviceProvider);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when calling PostInit for {service}", service.GetType().FullName);
            }
        }
    }

    public void Shutdown()
    {
        foreach (var service in _serviceProvider.GetServices<IManager>())
        {
            try
            {
                service.Shutdown();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when calling Shutdown for {service}", service.GetType().FullName);
            }
        }

        foreach (var service in _serviceProvider.GetServices<IModule>())
        {
            try
            {
                service.Shutdown();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when calling Shutdown for {service}", service.GetType().FullName);
            }
        }

        _serviceProvider.ShutdownAllSharpExtensions();
    }

    public void OnAllModulesLoaded()
    {
        _bridge.GetLocalizerManager().LoadLocaleFile("level_rank", true);
    }

    private static IConfigurationRoot LoadConfiguration(string sharpPath)
    {
        var configPath = Path.Combine(Path.GetFullPath(sharpPath), "configs");

        return new ConfigurationBuilder()
               .SetBasePath(configPath)
               .AddJsonFile("levelrank.jsonc", false, false)
               .Build();
    }

    string IModSharpModule.DisplayName   => "LevelRank";
    string IModSharpModule.DisplayAuthor => "Nukoooo";
}