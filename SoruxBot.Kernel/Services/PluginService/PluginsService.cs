using Microsoft.Extensions.DependencyInjection;
using SoruxBot.Kernel.Bot;
using SoruxBot.Kernel.Interface;
using SoruxBot.SDK.Plugins.Service;
using SoruxBot.Kernel.Services.StorageService;

namespace SoruxBot.Kernel.Services.PluginService
{
    public class PluginsService(BotContext context, ILoggerService loggerService)
    {
        /// <summary>
        /// 注册所有插件
        /// </summary>
        public void RegisterPlugins()
        {
            loggerService.Info(Constant.NameValue.KernelPluginServiceLogName, "Plugin Service is loading.");
            var rootDir = context.Configuration.GetSection("storage:root").Value!;
            var pluginsDir = Path.Join(rootDir, "plugins");
            var pluginsBin = Path.Join(pluginsDir, "bin");
            var pluginsConfig = Path.Join(pluginsDir, "config");
            PluginsRegister pluginsRegister = context.ServiceProvider.GetRequiredService<PluginsRegister>();

			var plugins = new DirectoryInfo(pluginsBin).GetFiles().ToList();
            // 注册插件
            plugins.ForEach(plugin => pluginsRegister.Register(plugin.Name, plugin.FullName,
                    Path.Join(pluginsConfig, plugin.Name.Replace(".dll", ".json"))));
			// 注册路由，完成优先级调度
			var pluginsStorage = context.ServiceProvider.GetRequiredService<IPluginsStorage>();
			foreach(var pluginName in pluginsStorage.GetPluginsOrderedByPrivilegeAsc())
			{
				var pluginInfo = plugins.First(p => p.Name == pluginName);
				pluginsRegister.RegisterRoute(pluginInfo.Name, pluginInfo.FullName);
			}
		}
        
        /// <summary>
        /// 注册所有的类库
        /// </summary>
        public void RegisterLibs()
        {
            loggerService.Info(Constant.NameValue.KernelPluginServiceLogName, "Plugin Service is loading.");
            var rootDir = context.Configuration.GetSection("storage:root").Value!;
            var pluginsDir = Path.Join(rootDir, "plugins");
            var pluginLibs = Path.Join(pluginsDir, "lib");
            PluginsRegister pluginsRegister = context.ServiceProvider.GetRequiredService<PluginsRegister>();

            // 注册插件类库
            new DirectoryInfo(pluginLibs)
                .GetFiles()
                .ToList()
                .ForEach(sp =>
                {
                    // 注册插件类库的逻辑
                    pluginsRegister.RegisterLib(sp.Name, sp.FullName);
                });
        }

        /// <summary>
        /// 配置插件服务，在Shell不启用插件的时候不进行加载，以防止向容器注入插件服务，反而导致错误在同游其他地方的子模块报错
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigurePluginsServices(IServiceCollection services)
        {
            //注册基础服务
            services.AddSingleton<PluginsService>();
            services.AddSingleton<PluginsRegister>();
            services.AddSingleton<IPluginsStorage, PluginsLocalStorage>();
            services.AddSingleton<IPluginsDataStorage, PluginsDataStorage>();

            //注册基础服务    
            services.AddSingleton<PluginsDispatcher>();
            services.AddSingleton<PluginsCommandLexer>();

            //添加API服务
            //services.AddSingleton<IBasicAPI, BasicApi>();
            //services.AddSingleton<ILongMessageCommunicate, LongMessageCommunicate>();

            //添加监听服务
            services.AddSingleton<PluginsListener>();
        }
    }
}