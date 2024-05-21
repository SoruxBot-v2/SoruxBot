using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SoruxBot.Kernel.Bot
{
    public interface IBotBuilder
    {
        //用于Bot配置项的生成
        IBotBuilder ConfigureBotConfiguration(Action<IConfigurationBuilder> configureDelegate);

        //用于Bot服务的注册生成
        IBotBuilder ConfigureServices(Action<IConfiguration, IServiceCollection> configureDelegate);

        //用于Bot实例的生成
        IBot Build();
    }
}