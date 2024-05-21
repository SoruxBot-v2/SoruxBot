using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SoruxBot.Kernel.Bot
{
    public class BotContext
    {
        private BotContext() { }

        //单例模型
        public static BotContext Instance { get; } = new BotContext();

        // IOC
        private IServiceCollection? _services;
        
        public IServiceProvider ServiceProvider { get; private set; }
        
        public IConfiguration Configuration { get; private set; }
        
        public BotContext CreateContainer(IServiceCollection serviceCollection)
        {
            _services = serviceCollection;
            return this;
        }
        
        public BotContext CreateIConfiguration(IConfiguration builder)
        {
            Configuration = builder;
            return this;
        }

        public BotContext ConfigureService(Action<IServiceCollection> services)
        {
            services(_services!);
            return this;
        }

        public BotContext BuildContainer()
        {
            ServiceProvider = _services!.BuildServiceProvider();
            return this;
        }
    }
}