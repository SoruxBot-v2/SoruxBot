using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SoruxBot.Kernel.Bot
{
    public class BotBuilder : IBotBuilder
    {
        //被建造的对象列表
        private readonly List<Action<IConfigurationBuilder>> _configureBotActions = new();
        
        private readonly List<Action<IConfiguration, IServiceCollection>> _configureServicesActions = new();

        //机器人配置，用于配置机器人项
        private IConfiguration? _appConfiguration;
        
        private readonly BotContext _botContext = BotContext.Instance;

        public IBot Build()
        {
            //初始化 Bot 配置
            InitBotConfiguration();
            //初始化 IOC 容器
            InitServices();
            //注册 Bot 配置的接口
            
            _botContext.ConfigureService(services =>
            {
                services.AddSingleton(_botContext);
                services.AddSingleton(_appConfiguration!);
            });
            
            _botContext.BuildContainer();
            
            return _botContext.ServiceProvider.GetRequiredService<IBot>();
        }

        private void InitServices()
        {
            var services = new ServiceCollection();
            foreach (Action<IConfiguration, IServiceCollection> configureServicesAction in _configureServicesActions)
            {
                configureServicesAction(_appConfiguration!, services);
            }

            _botContext.CreateContainer(services);
            _botContext.CreateIConfiguration(_appConfiguration!);
        }

        private void InitBotConfiguration()
        {
            IConfigurationBuilder configBuilder = new ConfigurationBuilder();
            
            foreach (var buildAction in _configureBotActions)
            {
                buildAction(configBuilder);
            }

            _appConfiguration = configBuilder.Build();
        }

        public IBotBuilder ConfigureBotConfiguration(Action<IConfigurationBuilder> configureDelegate)
        {
            _configureBotActions.Add(configureDelegate);
            return this;
        }

        public IBotBuilder ConfigureServices(Action<IConfiguration, IServiceCollection> configureDelegate)
        {
            _configureServicesActions.Add(configureDelegate);
            return this;
        }
    }
}