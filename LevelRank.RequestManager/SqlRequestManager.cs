using LevelRank.Shared;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace LevelRank.Request.Sql;

public class SqlRequestManager : IRequestManager, IDisposable
{
    private readonly SqlSugarScope              _db;
    private readonly ILogger<SqlRequestManager> _logger;

    public SqlRequestManager(ConnectionConfig connectionConfig, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<SqlRequestManager>();
        _db     = new SqlSugarScope(connectionConfig);
    }

    public void Init()
    {
        _db.CodeFirst.InitTables<PlayerRankEntity>();
        _logger.LogInformation("SqlRequestManager initialized, tables created if not exist");
    }

    public async Task<RankInfo> GetUserRankInfo(ulong steamId)
    {
        try
        {
            var entity = await _db.Queryable<PlayerRankEntity>()
                                  .FirstAsync(x => x.SteamId == steamId);

            return entity?.ToRankInfo() ?? new RankInfo();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user rank info for SteamId {SteamId}", steamId);

            throw;
        }
    }

    public async Task UpdateUserInfo(ulong steamId, RankInfo info)
    {
        try
        {
            var entity = PlayerRankEntity.FromRankInfo(steamId, info);

            await _db.Storageable(entity)
                     .WhereColumns(x => x.SteamId)
                     .ExecuteCommandAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user info for SteamId {SteamId}", steamId);

            throw;
        }
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
