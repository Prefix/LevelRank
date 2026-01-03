using LevelRank.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sharp.Modules.LocalizerManager.Shared;
using Sharp.Shared;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;

namespace LevelRank;

internal interface IModule
{
    bool Init();

    void OnPostInit(ServiceProvider provider)
    {
    }

    void Shutdown()
    {
    }
}

internal interface IManager
{
    bool Init();

    void OnPostInit(ServiceProvider provider)
    {
    }

    void Shutdown();
}

internal class InterfaceBridge
{
    private readonly ISharedSystem _sharedSystem;

    public InterfaceBridge(string        dllPath,
                           string        sharpPath,
                           Version       version,
                           ISharedSystem sharedSystem,
                           bool          hotReload,
                           bool          debug)
    {
        DllPath       = dllPath;
        SharpPath     = sharpPath;
        Version       = version;
        _sharedSystem = sharedSystem;
        HotReload     = hotReload;
        Debug         = debug;

        ModSharp        = sharedSystem.GetModSharp();
        ConVarManager   = sharedSystem.GetConVarManager();
        EventManager    = sharedSystem.GetEventManager();
        ClientManager   = sharedSystem.GetClientManager();
        EntityManager   = sharedSystem.GetEntityManager();
        FileManager     = sharedSystem.GetFileManager();
        HookManager     = sharedSystem.GetHookManager();
        SchemaManager   = sharedSystem.GetSchemaManager();
        TransmitManager = sharedSystem.GetTransmitManager();
        SteamAPi        = ModSharp.GetSteamGameServer();

        ModuleManager   = sharedSystem.GetLibraryModuleManager();
        EconItemManager = sharedSystem.GetEconItemManager();

        PhysicsQueryManager = sharedSystem.GetPhysicsQueryManager();
        SoundManager        = sharedSystem.GetSoundManager();

        SharpModule = sharedSystem.GetSharpModuleManager();
    }

    public string DllPath { get; }

    public string SharpPath { get; }

    public Version Version { get; }

    public bool HotReload { get; }

    public bool Debug { get; }

    public IModSharp        ModSharp        { get; }
    public IConVarManager   ConVarManager   { get; }
    public IEventManager    EventManager    { get; }
    public IClientManager   ClientManager   { get; }
    public IEntityManager   EntityManager   { get; }
    public IEconItemManager EconItemManager { get; }
    public IFileManager     FileManager     { get; }
    public IHookManager     HookManager     { get; }
    public ISchemaManager   SchemaManager   { get; }
    public ITransmitManager TransmitManager { get; }
    public ISteamApi        SteamAPi        { get; }

    public IPhysicsQueryManager PhysicsQueryManager { get; }

    public ISoundManager         SoundManager  { get; }
    public ILibraryModuleManager ModuleManager { get; }

    private ISharpModuleManager SharpModule { get; }

    public IGameRules     GameRules     => ModSharp.GetGameRules();
    public IGlobalVars    GlobalVars    => ModSharp.GetGlobals();
    public INetworkServer Server        => ModSharp.GetIServer();
    public ILoggerFactory LoggerFactory => _sharedSystem.GetLoggerFactory();

    private IRequestManager?   _cachedRequestManager;
    private ILocalizerManager? _cachedLocalizerManager;

    public IRequestManager GetRequestManager()
    {
        if (_cachedRequestManager is not null)
        {
            return _cachedRequestManager;
        }

        var iface = SharpModule.GetRequiredSharpModuleInterface<IRequestManager>(IRequestManager.Identity);

        if (iface is { IsAvailable: true, Instance: { } instance })
        {
            _cachedRequestManager = instance;

            return instance;
        }

        throw new
            InvalidOperationException($"Required module '{IRequestManager.Identity}' could not be loaded or is unavailable.");
    }

    public ILocalizerManager GetLocalizerManager()
    {
        if (_cachedLocalizerManager is not null)
        {
            return _cachedLocalizerManager;
        }

        var iface = SharpModule.GetRequiredSharpModuleInterface<ILocalizerManager>(ILocalizerManager.Identity);

        if (iface is { IsAvailable: true, Instance: { } instance })
        {
            _cachedLocalizerManager = instance;

            return instance;
        }

        throw new
            InvalidOperationException($"Required module '{ILocalizerManager.Identity}' could not be loaded or is unavailable.");
    }
}
