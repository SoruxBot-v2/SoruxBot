using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SoruxBot.Kernel.Bot;
using SoruxBot.Kernel.Interface;
using SoruxBot.Kernel.Services.PluginService.Model;
using SoruxBot.SDK.Plugins.Ability;
using SoruxBot.SDK.Plugins.Basic;
using SoruxBot.SDK.Plugins.Service;

namespace SoruxBot.Kernel.Services.PluginService
{
    internal class PluginsRegister(BotContext context, ILoggerService loggerService)
    {
        public void Register(string name, string instancePath, string configPath)
        {
            var assembly = Assembly.LoadFile(instancePath);

            //命名空间规定为 Register
            var type = assembly.GetType(name.Replace(".dll", ".Register"));

            if (type == null)
            {
                loggerService.Error(Constant.NameValue.KernelPluginServiceRegisterLogName, "The plugin:" + name +
                    "can not be loaded exactly" +
                    ", due to its wrong type. Please check its Register namespace exist or not.");
                return;
            }

            object pluginInstance = Activator.CreateInstance(type)!;

            var soruxBotPlugin = pluginInstance as SoruxBotPlugin;

            if (soruxBotPlugin is null)
            {
                loggerService.Error(Constant.NameValue.KernelPluginServiceRegisterLogName, "The plugin:" + name +
                    "can not be loaded exactly" +
                    ", due to the Register namespace is not the child of SoruxBotPlugin.");
                return;
            }

            PluginJsonConfig? pluginConfig;
            try
            {
                pluginConfig = JsonSerializer.Deserialize<PluginJsonConfig>(File.ReadAllText(configPath));
            }
            catch (Exception ex)
            {
                loggerService.Error(Constant.NameValue.KernelPluginServiceRegisterLogName, "The plugin:" + name +
                    " cannot load json file, throwing " +
                    $"error: {ex.Message}");
                return;
            }

            if (pluginConfig is null)
            {
                loggerService.Error(Constant.NameValue.KernelPluginServiceRegisterLogName,
                    "The plugin:" + name + " cannot load json file");
                return;
            }

            var pluginsStorage = context.ServiceProvider.GetRequiredService<IPluginsStorage>();

            pluginsStorage.AddPlugin(
                soruxBotPlugin.GetPluginName(),
                soruxBotPlugin.GetPluginAuthorName(),
                name,
                soruxBotPlugin.GetPluginVersion(),
                soruxBotPlugin.GetPluginDescription(),
                pluginConfig.Privilege,
                soruxBotPlugin.IsUpperWhenPrivilege()
            );

            loggerService.Info(Constant.NameValue.KernelPluginServiceRegisterLogName, "Loading plugin:" + name);

            // Register 注册可选特性
            pluginsStorage.SetPluginInformation(name, "CommandPrefix", "false");

            if (pluginInstance is ICommandPrefix prefix)
            {
                pluginsStorage.SetPluginInformation(name, "CommandPrefix", "true");
                pluginsStorage.SetPluginInformation(name, "CommandPrefixContent", prefix.GetPluginPrefix());
            }

            // 存储插件实例
            pluginsStorage.SetPluginInstance(name, soruxBotPlugin);
        }

        /// <summary>
        /// 注册插件类库
        /// </summary>
        /// <param name="name"></param>
        /// <param name="instancePath"></param>
        public void RegisterLib(string name, string instancePath)
        {
            var assembly = Assembly.LoadFile(instancePath);

            //命名空间规定为 Register
            var registerType = assembly.GetType(name.Replace(".dll", ".Register"));

            if (registerType == null)
            {
                loggerService.Error(Constant.NameValue.KernelPluginServiceRegisterLogName, "The lib:" + name +
                    "can not be loaded exactly" +
                    ", due to its wrong type. Please check its Register namespace exist or not.");
                return;
            }

            object pluginInstance = Activator.CreateInstance(registerType)!;

            var soruxBotLib = pluginInstance as SoruxBotLib;

            if (soruxBotLib is null)
            {
                loggerService.Error(Constant.NameValue.KernelPluginServiceRegisterLogName, "The lib:" + name +
                    "can not be loaded exactly" +
                    ", due to the Register namespace is not the child of SoruxBotLib.");
                return;
            }

            // 检查完毕，然后开始注册
            var typeLists = assembly.GetExportedTypes();

            var typeMap = new Dictionary<Type, Type?>();
            
            // 遍历类
            foreach (var type in typeLists)
            {
                if (typeof(IPluginLibInterface).IsAssignableFrom(type) && type.IsInterface)
                {
                    loggerService.Debug(Constant.NameValue.KernelPluginServiceRegisterLogName,
                        "Plugin Lib is caught! Type ->" + type.Name);
                    typeMap.Add(type, null);
                }
            }
            
            foreach (var type in typeLists)
            {
                foreach (var interfaceType in typeMap.Keys.ToList())
                {
                    if (interfaceType.IsAssignableFrom(type) && 
                        type is { IsInterface: false, IsAbstract: false })
                    {
                        loggerService.Debug(Constant.NameValue.KernelPluginServiceRegisterLogName,
                            "Type implementing Plugin Lib is caught! Type ->" + type.Name);
                        typeMap[interfaceType] = type;
                    }
                }
            }
            
            // 注册 IOC 容器
            foreach (var entry in typeMap)
            {
                if (entry.Value != null)
                {
                    context.ServiceProvider.GetRequiredService<PluginsDispatcher>().Services
                        .AddSingleton(entry.Key, entry.Value);
                    loggerService.Debug(Constant.NameValue.KernelPluginServiceDispatcherLogName,
                        $"Registered {entry.Key.Name} with implementation {entry.Value.Name}");
                }
                else
                {
                    loggerService.Warn(Constant.NameValue.KernelPluginServiceDispatcherLogName,
                        $"No implementation found for {entry.Key.Name}");
                }
            }
            
            context.ServiceProvider.GetRequiredService<PluginsDispatcher>().ServiceProvider =
                context.ServiceProvider.GetRequiredService<PluginsDispatcher>().Services.BuildServiceProvider();
        }

        /// <summary>
        /// 注册路由
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        public void RegisterRoute(string name, string path)
        {
            context.ServiceProvider.GetRequiredService<PluginsDispatcher>().RegisterCommandRoute(path, name);
        }
    }
}