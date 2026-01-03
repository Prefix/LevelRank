using LevelRank.Shared;

namespace LevelRank.Managers;

internal struct PlayerData
{
    public ulong SteamId { get; init; }

    public RankInfo RankInfo { get; init; }

    public bool IsDirty { get; set; }

    public double ConnectTime { get; init; }

    public bool IsValid => RankInfo is not null;
}
