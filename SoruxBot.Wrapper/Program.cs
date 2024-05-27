using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.DependencyInjection;
using SoruxBot.Kernel.Bot;
using SoruxBot.Kernel.Interface;
using SoruxBot.SDK.Plugins.Service;
using SoruxBot.WebGrpc;
using SoruxBot.Wrapper.Service;

var app = CreateDefaultBotBuilder(args)
                .Build();

// 构建 gRpc 服务
BuildGrpcServer(app).Start();

const string loggerName = "SoruxBot.Wrapper";
var logger = app.Context.ServiceProvider.GetRequiredService<ILoggerService>();

logger.Info(loggerName, $"SoruxBot Current Kernel Version: {app.Context.Configuration.GetSection("SDKVersion").Value}")
      .Info(loggerName, $"Running path: {app.Context.Configuration.GetSection("CurrentPath").Value}")
      .Info(loggerName, $"Development logger state: {app.Context.Configuration.GetSection("LoggerDebug").Value}")
      .Info(loggerName, $"SoruxBot running root path: {app.Context.Configuration.GetSection("storage:root").Value}");


static IBotBuilder CreateDefaultBotBuilder(string[] args)
{
    BotBuilder botBuilder = new();
    return botBuilder
        .CreateDefaultBotConfigure(args)
        .ConfigureBotConfiguration(config =>
        {
            // 添加 config.yaml 文件
            config.AddYamlFile("config.yaml", optional: false, reloadOnChange: true);
        })
        .ConfigureBotConfiguration(config =>
        {
            config.AddInMemoryCollection(new[]
            {
                //本设置的 Debug 针对于框架内部，一般情况下不需要开启本项，即使是生产环境的调试，如果是开发框架，建议打开
                new KeyValuePair<string, string?>("LoggerDebug", "true")
            });
        });
}

static Server BuildGrpcServer(IBot app)
    => new Server
    {
        Services =
        {
            Message.BindService(
                new MessageService(
                    app.Context.ServiceProvider.GetRequiredService<ILoggerService>(), 
                    app.Context.ServiceProvider.GetRequiredService<IMessageQueue>(),
                    app.Context.Configuration.GetSection("chat:token").Value
                    ))
        },
        
        Ports = { 
            new ServerPort(
            app.Context.Configuration.GetSection("chat:host").Value, 
            int.Parse(app.Context.Configuration.GetSection("chat:port").Value!),
            ServerCredentials.Insecure) 
        }
    };