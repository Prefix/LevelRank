using LevelRank.Managers;
using Sharp.Shared.Enums;
using Sharp.Shared.HookParams;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;

namespace LevelRank.Modules;

internal class ScoreboardModule : IModule
{
    private readonly InterfaceBridge _bridge;
    private readonly IPlayerManager  _playerManager;
    private readonly IConVar         _showScoreInScoreboard;

    private static readonly CCSUsrMsg_ServerRankRevealAll ServerRankRevealAll = new ();

    public ScoreboardModule(InterfaceBridge bridge, IPlayerManager playerManager, IConfigManager configManager)
    {
        _bridge        = bridge;
        _playerManager = playerManager;

        _showScoreInScoreboard = configManager.CreateConVar("lr_show_score_in_scoreboard",
                                                            true,
                                                            "Should show player's score in scoreboard with premier style");
    }

    public bool Init()
    {
        _bridge.HookManager.PlayerRunCommand.InstallHookPost(OnPlayerRunCommandPost);

        return true;
    }

    public void Shutdown()
    {
        _bridge.HookManager.PlayerRunCommand.RemoveHookPost(OnPlayerRunCommandPost);
    }

    private void OnPlayerRunCommandPost(IPlayerRunCommandHookParams @params, HookReturnValue<EmptyHookReturn> ret)
    {
        var client = @params.Client;

        if (_playerManager.GetPlayerRankInfo(@params.Client) is not { } rank || rank.Score == 0)
        {
            return;
        }

        var controller = @params.Controller;

        if (_showScoreInScoreboard.GetBool())
        {
            controller.CompetitiveRankType = CompetitiveRankType.Primer;
        }
        else
        {
            controller.CompetitiveRankType = 0;
            controller.CompetitiveWins     = 0;
            controller.CompetitiveRanking  = 0;

            return;
        }

        controller.CompetitiveWins    = 10;
        controller.CompetitiveRanking = (int) rank.Score;

        var service = @params.Service;

        if ((service.KeyButtons           & UserCommandButtons.Scoreboard) == 0
            || (service.KeyChangedButtons & UserCommandButtons.Scoreboard) != 0)
        {
            return;
        }

        var filter = new RecipientFilter(client);
        _bridge.ModSharp.SendNetMessage(filter, ServerRankRevealAll);
    }
}
