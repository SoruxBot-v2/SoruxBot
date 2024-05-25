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
                loggerService.Error(Constant.NameValue.KernelPluginServiceLogName, "The plugin:" + name + "can not be loaded exactly" +
                                                        ", due to its wrong type. Please check its Register namespace exist or not.");
                return;
            }

            object pluginInstance = Activator.CreateInstance(type)!;
            
            var soruxBotPlugin = pluginInstance as SoruxBotPlugin;
            
            if (soruxBotPlugin is null)
            {
                loggerService.Error(Constant.NameValue.KernelPluginServiceLogName, "The plugin:" + name + "can not be loaded exactly" +
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
                loggerService.Error(Constant.NameValue.KernelPluginServiceLogName, "The plugin:" + name + " cannot load json file, throwing " +
                                                       $"error: {ex.Message}");
                return;
            }

            if (pluginConfig is null)
            {
                loggerService.Error(Constant.NameValue.KernelPluginServiceLogName,
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

            loggerService.Info(Constant.NameValue.KernelPluginServiceLogName, "Loading plugin:" + name);

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