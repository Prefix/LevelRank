namespace LevelRank.Shared;

public interface IRequestManager
{
    const string Identity = "LevelRank.IRequestManager";

    Task<RankInfo> GetUserRankInfo(ulong steamId);

    Task UpdateUserInfo(ulong steamId, RankInfo info);
}