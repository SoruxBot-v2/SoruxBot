using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoruxBot.Kernel.Services;
using SoruxBot.SDK.Plugins.Service;

namespace SoruxBot.Kernel.Bot
{
    public static class BotBuilderExtension
    {
        public static IBotBuilder CreateDefaultBotConfigure(this IBotBuilder builder, string[]? args)
            => builder.ConfigureBotConfiguration(config => ApplyDefaultRuntimeConfiguration(config, args))
                //注入Bot配置信息，用于存储基本的框架配置信息
                .ConfigureBotConfiguration(ApplyDefaultBotConfiguration)
                //注入框架自身的版本信息
                .ConfigureBotConfiguration(ApplyDefaultBotFrameworkInformation)
                //配置框架的基本服务信息
                .ConfigureServices(ApplyDefaultServices)
                //注入不可变的信息，分开放是因为这四个服务是绝对不会变的
                .ConfigureServices((config, services) =>
                {
                    //Bot自身
                    services.AddSingleton<IBot, SoruxBot.Kernel.Bot.Bot>();
                    var loggerFactory = LoggerFactory.Create(loggingBuilder =>
                    {
                        loggingBuilder.AddConsole();
                        if (config["LoggerDebug"] == "true")
                            loggingBuilder.AddDebug();

                        services.AddSingleton(loggingBuilder);
                    });
                    
                    services.AddSingleton(loggerFactory);
                    services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
                    services.AddSingleton<ILoggerService, LoggerService>();
                });

        private static void ApplyDefaultRuntimeConfiguration(IConfigurationBuilder config, string[]? args)
        {
            //添加 CommandLine 的 args
            if (args is { Length: > 0 })
            {
                config.AddCommandLine(args);
            }
        }

        private static void ApplyDefaultBotConfiguration(IConfigurationBuilder config)
        {
            //Bot Configuration主要负责配置连接器的组装等操作
            string cwd = Environment.CurrentDirectory;
            config.AddYamlFile("config.yaml", optional: false, reloadOnChange: true);
            
            config.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("CurrentPath", cwd)
            });
        }

        private static void ApplyDefaultBotFrameworkInformation(IConfigurationBuilder config)
        {
            //注入基本信息
            config.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("SDKVersion", SDK.Manifests.Manifests.SdkMainVersion),
            });
        }

        private static void ApplyDefaultServices(IConfiguration configuration, IServiceCollection services)
        {
            // 添加实例
            
        }
    }
}