using LevelRank.Managers;
using Microsoft.Extensions.Logging;
using Sharp.Extensions.GameEventManager;
using Sharp.Shared.Enums;
using Sharp.Shared.GameEvents;
using Sharp.Shared.Objects;

namespace LevelRank.Modules;

internal class RoundModule : IModule
{
    private readonly InterfaceBridge      _bridge;

    private readonly IGameEventManager _gameEventManager;
    private readonly IPlayerManager    _playerManager;

    private readonly IScoreModule   _scoreModule;
    private readonly IMessageModule _messageModule;

    private readonly ILogger<RoundModule> _logger;

    public RoundModule(InterfaceBridge      bridge,
                       IPlayerManager       playerManager,
                       IGameEventManager    gameEventManager,
                       IScoreModule         scoreModule,
                       IMessageModule       messageModule,
                       ILogger<RoundModule> logger)
    {
        _bridge           = bridge;

        _gameEventManager = gameEventManager;
        _playerManager    = playerManager;

        _logger        = logger;
        _scoreModule   = scoreModule;
        _messageModule = messageModule;
    }

    public bool Init()
    {
        _gameEventManager.ListenEvent("round_end", OnRoundEnd);

        return true;
    }

    private void OnRoundEnd(IGameEvent e)
    {
        if (!_scoreModule.IsRankEnabled)
        {
            return;
        }

        if (e is not IEventRoundEnd ev)
        {
            return;
        }

        var winnerTeam = ev.Winner;

        if (winnerTeam is CStrikeTeam.UnAssigned or CStrikeTeam.Spectator)
        {
            return;
        }

        var roundWinScore    = _scoreModule.GetScoreForAction(ScoreAction.RoundWins);
        var roundLossesScore = _scoreModule.GetScoreForAction(ScoreAction.RoundLosses);

        foreach (var client in _bridge.ClientManager.GetGameClients(true))
        {
            if (client.IsFakeClient || client.IsHltv)
            {
                continue;
            }

            if (client.GetPlayerController() is not { } controller || controller.GetPlayerPawn() is not { } pawn)
            {
                continue;
            }

            if (_playerManager.GetPlayerRankInfo(client) is not { } rankInfo)
            {
                continue;
            }

            var team = pawn.Team;

            if (team <= CStrikeTeam.Spectator)
            {
                continue;
            }

            var isWinner = team == winnerTeam;

            if (isWinner)
            {
                rankInfo.RoundWins++;
                _playerManager.UpdateScore(client, roundWinScore);
                _messageModule.SendScoreUpdate(client, rankInfo.Score, [ScoreAction.RoundWins]);
            }
            else
            {
                rankInfo.RoundLosts++;
                _playerManager.UpdateScore(client, roundLossesScore);
                _messageModule.SendScoreUpdate(client, rankInfo.Score, [ScoreAction.RoundLosses]);
            }
        }
    }
}
