using LevelRank.Shared;
using SqlSugar;

namespace LevelRank.Request.Sql;

[SugarTable("player_ranks")]
public class PlayerRankEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public ulong SteamId { get; set; }

    public double PlayTime { get; set; }

    public ulong Score { get; set; }

    public ulong Kills { get; set; }
    public ulong Deaths { get; set; }
    public ulong Assists { get; set; }
    public ulong Headshots { get; set; }
    public ulong ZeusKills { get; set; }
    public ulong KnifeKills { get; set; }
    public ulong UtilitiesKills { get; set; }

    public ulong RoundWins { get; set; }
    public ulong RoundLosts { get; set; }

    public ulong BombPlants { get; set; }
    public ulong BombDefuses { get; set; }

    public ulong HostageRescues        { get; set; }
    public ulong PreventHostageRescues { get; set; }

    public RankInfo ToRankInfo()
        => new ()
        {
            PlayTime              = PlayTime,
            Score                 = Score,
            Kills                 = Kills,
            Deaths                = Deaths,
            Assists               = Assists,
            Headshots             = Headshots,
            ZeusKills             = ZeusKills,
            KnifeKills            = KnifeKills,
            UtilitiesKills        = UtilitiesKills,
            RoundWins             = RoundWins,
            RoundLosts            = RoundLosts,
            BombPlants            = BombPlants,
            BombDefuses           = BombDefuses,
            HostageRescues        = HostageRescues,
            PreventHostageRescues = PreventHostageRescues,
        };

    public static PlayerRankEntity FromRankInfo(ulong steamId, RankInfo info)
        => new ()
        {
            SteamId               = steamId,
            PlayTime              = info.PlayTime,
            Score                 = info.Score,
            Kills                 = info.Kills,
            Deaths                = info.Deaths,
            Assists               = info.Assists,
            Headshots             = info.Headshots,
            ZeusKills             = info.ZeusKills,
            KnifeKills            = info.KnifeKills,
            UtilitiesKills        = info.UtilitiesKills,
            RoundWins             = info.RoundWins,
            RoundLosts            = info.RoundLosts,
            BombPlants            = info.BombPlants,
            BombDefuses           = info.BombDefuses,
            HostageRescues        = info.HostageRescues,
            PreventHostageRescues = info.PreventHostageRescues,
        };
}
