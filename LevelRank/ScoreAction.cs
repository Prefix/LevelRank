namespace LevelRank;

internal enum ScoreAction
{
    // Kill-related
    Kills,
    Deaths,
    Suicides,
    Assists,
    Headshots,
    KnifeKills,
    ZeusKills,
    UtilitiesKills,
    PreventHostageRescue,

    // Bomb-related
    BombPlants,
    BombDefuses,

    // Hostage-related
    HostageRescues,

    // Round-related
    RoundWins,
    RoundLosses,
}
