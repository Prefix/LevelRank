using LevelRank.Managers;
using LevelRank.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace LevelRank;

internal static class DependencyInjections
{
    extension(IServiceCollection services)
    {
        private void ImplSingleton<TService1, TService2, TImpl>()
            where TImpl : class, TService1, TService2
            where TService1 : class
            where TService2 : class
        {
            services.AddSingleton<TImpl>();

            services.AddSingleton<TService1>(x => x.GetRequiredService<TImpl>());
            services.AddSingleton<TService2>(x => x.GetRequiredService<TImpl>());
        }

        public void AddManagerDi()
        {
            services.ImplSingleton<IConfigManager, IManager, ConfigManager>();
            services.ImplSingleton<IPlayerManager, IManager, PlayerManager>();
        }

        public void AddModuleDi()
        {
            services.ImplSingleton<IScoreModule, IModule, ScoreModule>();
            services.ImplSingleton<IMessageModule, IModule, MessageModule>();

            services.AddSingleton<IModule, DeathModule>();
            services.AddSingleton<IModule, RoundModule>();
            services.AddSingleton<IModule, BombModule>();
            services.AddSingleton<IModule, HostageModule>();
            services.AddSingleton<IModule, ScoreboardModule>();
        }
    }
}
