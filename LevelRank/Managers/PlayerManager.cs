using LevelRank.Shared;
using Microsoft.Extensions.Logging;
using Sharp.Extensions.GameEventManager;
using Sharp.Shared.Enums;
using Sharp.Shared.Listeners;
using Sharp.Shared.Objects;
using Sharp.Shared.Units;

namespace LevelRank.Managers;

internal interface IPlayerManager
{
    RankInfo? GetPlayerRankInfo(IGameClient client);

    RankInfo? GetPlayerRankInfo(PlayerSlot slot);

    void UpdateScore(IGameClient client, long scoreDelta);

    void UpdateScore(PlayerSlot slot, long scoreDelta);
}

internal class PlayerManager : IManager, IClientListener, IPlayerManager, IGameListener
{
    private readonly PlayerData[] _playerData = new PlayerData[64];

    private readonly InterfaceBridge   _bridge;
    private readonly IGameEventManager _gameEventManager;

    private readonly ILogger<PlayerManager> _logger;

    public PlayerManager(InterfaceBridge        bridge,
                         IGameEventManager      gameEventManager,
                         ILogger<PlayerManager> logger)
    {
        _bridge           = bridge;
        _gameEventManager = gameEventManager;
        _logger           = logger;
    }

    public bool Init()
    {
        _bridge.ClientManager.InstallClientListener(this);
        _bridge.ModSharp.InstallGameListener(this);

        return true;
    }

    public void Shutdown()
    {
        _bridge.ClientManager.RemoveClientListener(this);
        _bridge.ModSharp.RemoveGameListener(this);

        var requestManager = _bridge.GetRequestManager();

        var time = _bridge.ModSharp.EngineTime();

        foreach (var data in _playerData)
        {
            if (!data.IsValid)
            {
                continue;
            }

            data.RankInfo.PlayTime += time - data.ConnectTime;

            try
            {
                requestManager.UpdateUserInfo(data.SteamId, data.RankInfo).Wait();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist data for player {SteamId} on shutdown", data.SteamId);
            }
        }
    }

    public void OnClientPutInServer(IGameClient client)
    {
        if (client.IsFakeClient || client.IsHltv)
        {
            return;
        }

        var steamId = client.SteamId;

        var requestManager = _bridge.GetRequestManager();

        var time = _bridge.ModSharp.EngineTime();

        _ = Task.Run(async () =>
        {
            try
            {
                var rankInfo = await requestManager.GetUserRankInfo(steamId).ConfigureAwait(false);

                var playerData = new PlayerData
                {
                    SteamId     = steamId,
                    RankInfo    = rankInfo,
                    IsDirty     = false,
                    ConnectTime = time,
                };

                await _bridge.ModSharp.InvokeFrameActionAsync(() =>
                {
                    if (_bridge.ClientManager.GetGameClient(steamId) is not { } cl)
                    {
                        return;
                    }

                    _playerData[cl.Slot] = playerData;
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load data for player {SteamId}", steamId);
            }
        });
    }

    public void OnClientDisconnected(IGameClient client, NetworkDisconnectionReason reason)
    {
        if (client.IsFakeClient || client.IsHltv)
        {
            return;
        }

        if (!_playerData[client.Slot].IsValid)
        {
            return;
        }

        var slot = client.Slot;
        var steamId = client.SteamId;
        var rankInfo = _playerData[slot].RankInfo;

        var requestManager = _bridge.GetRequestManager();
        var time           = _bridge.ModSharp.EngineTime();

        rankInfo.PlayTime += time - _playerData[slot].ConnectTime;

        _ = Task.Run(async () =>
        {
            try
            {
                await requestManager.UpdateUserInfo(steamId, rankInfo).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist data for player {SteamId} on disconnect", steamId);
            }
        });

        _playerData[slot] = default;
    }

    public void OnRoundRestart()
    {
        var requestManager = _bridge.GetRequestManager();

        Task.Run(async () =>
        {
            for (var i = 0; i < _playerData.Length; i++)
            {
                if (!_playerData[i].IsValid || !_playerData[i].IsDirty)
                {
                    continue;
                }

                var steamId = _playerData[i].SteamId;
                var rankInfo = _playerData[i].RankInfo;

                try
                {
                    await requestManager.UpdateUserInfo(steamId, rankInfo).ConfigureAwait(false);
                    _playerData[i].IsDirty = false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to persist data for player {SteamId} on OnRoundRestart", steamId);
                }
            }
        });
    }

    public RankInfo? GetPlayerRankInfo(IGameClient client)
        => GetPlayerRankInfo(client.Slot);

    public RankInfo? GetPlayerRankInfo(PlayerSlot slot)
        => !_playerData[slot].IsValid ? null : _playerData[slot].RankInfo;

    public void UpdateScore(IGameClient client, long scoreDelta)
    {
        UpdateScore(client.Slot, scoreDelta);
    }

    public void UpdateScore(PlayerSlot slot, long scoreDelta)
    {
        ref var data = ref _playerData[slot];
        if (!data.IsValid || scoreDelta == 0)
        {
            return;
        }

        var rank      = data.RankInfo;
        var tempScore = (Int128) rank.Score + scoreDelta;
        var newScore  = (ulong) Int128.Clamp(tempScore, 0, ulong.MaxValue);

        if (rank.Score == newScore)
        {
            return;
        }

        rank.Score   = newScore;
        data.IsDirty = true;
    }

    public int ListenerVersion => IClientListener.ApiVersion;
    public int ListenerPriority => 1;
}
