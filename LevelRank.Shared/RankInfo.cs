namespace LevelRank.Shared;

public class RankInfo
{
    // in second
    public double PlayTime { get; set; }

    public ulong Score { get; set; }

    public ulong Kills          { get; set; }
    public ulong Deaths         { get; set; }
    public ulong Assists        { get; set; }
    public ulong Headshots      { get; set; }
    public ulong ZeusKills      { get; set; }
    public ulong KnifeKills     { get; set; }
    public ulong UtilitiesKills { get; set; }

    public ulong RoundWins  { get; set; }
    public ulong RoundLosts { get; set; }

    public ulong BombPlants  { get; set; }
    public ulong BombDefuses { get; set; }

    public ulong HostageRescues { get; set; }
    public ulong HostageCarrierKills { get; set; }
}