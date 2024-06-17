using System.Collections.Concurrent;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.DependencyInjection;
using SoruxBot.Kernel.Bot;
using SoruxBot.Kernel.Interface;
using SoruxBot.Kernel.Services.PluginService;
using SoruxBot.SDK.Plugins.Service;
using SoruxBot.Kernel.Services.PushService;
using SoruxBot.SDK.Model.Message;
using SoruxBot.Wrapper.Service;
using Grpc.Net.Client;
using Newtonsoft.Json;
using SoruxBot.Provider.WebGrpc;
using SoruxBot.SDK.Plugins.Model;

var app = CreateDefaultBotBuilder(args)
    .Build();

// 构建 gRpc 服务
BuildGrpcServer(app).Start();

// 构建 grpc 客户端
var configuration = new ConfigurationBuilder()
    .AddYamlFile("config.yaml", optional: false, reloadOnChange: true)
    .Build();

// 构建 gRpc 客户端池
var grpcClients = new ConcurrentDictionary<string, Tuple<string, Message.MessageClient>>();

// 获取yaml中的数组
configuration.GetSection("provider").GetChildren().ToList().ForEach(
    x =>
    {
        var grpcChannel = GrpcChannel.ForAddress(
            $"http://{x.GetSection("host").Value}");

        grpcClients[$"{x.GetSection("type").Value?.ToLower()}@{x.GetSection("account").Value}"]
            = new Tuple<string, Message.MessageClient>(
                $"{x.GetSection("token").Value}",
                new Message.MessageClient(grpcChannel)
            );
    });

// 注册插件类库
app.Context.ServiceProvider.GetRequiredService<PluginsService>().RegisterLibs();

// 注册插件服务
app.Context.ServiceProvider.GetRequiredService<PluginsService>().RegisterPlugins();

const string loggerName = "SoruxBot.Wrapper";
var logger = app.Context.ServiceProvider.GetRequiredService<ILoggerService>();

logger.Info(loggerName, $"SoruxBot Current Kernel Version: {app.Context.Configuration.GetSection("SDKVersion").Value}")
    .Info(loggerName, $"Running path: {app.Context.Configuration.GetSection("CurrentPath").Value}")
    .Info(loggerName, $"Development logger state: {app.Context.Configuration.GetSection("LoggerDebug").Value}")
    .Info(loggerName, $"SoruxBot running root path: {app.Context.Configuration.GetSection("storage:root").Value}");
var jsonSettings = new JsonSerializerSettings
{
    TypeNameHandling = TypeNameHandling.All
};

var pushService = app.Context.ServiceProvider.GetRequiredService<IPushService>();
{
    var pluginsDispatcher =
        app.Context.ServiceProvider.GetRequiredService<PluginsDispatcher>();
    var pluginsCommandLexer =
        app.Context.ServiceProvider.GetRequiredService<PluginsCommandLexer>();

    pushService.RunInstance(
        context =>
        {
			bool msgIntercepted = false;
            pluginsDispatcher.GetAction(ref context)?.ForEach(
                sp =>
                {
					if (msgIntercepted) return;
                    try
                    {
                        if(pluginsCommandLexer.PluginAction(context, sp) == PluginFlag.MsgIntercepted)
						{
							msgIntercepted = true;
						}
                    }
                    catch (Exception e)
                    {
                        logger.Error(loggerName, $"Plugin Error catch, with exception: {e.Message} and context {context}");
                    }
                });
        },
        context =>
        {
            // 这里利用MessageContext，从Provider得到MessageId
            var tuple = grpcClients[context.TargetPlatform.ToLower() + "@" + context.BotAccount];
            var response = tuple.Item2
                .MessageSend(new MessageRequest
                {
                    Payload = JsonConvert.SerializeObject(context, jsonSettings),
                    // TODO 这里需要从配置文件中读取
                    Token = tuple.Item1
                });

            // 拿到MessageResult
            var result = JsonConvert
                .DeserializeObject<MessageResult>(response.Payload, jsonSettings);

            // 不会为空
            return result!;
        });
}


logger.Info(loggerName, "SoruxBot has been initialized.");

await Task.Delay(-1);

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
        })
        .ConfigureServices((_, service) =>
        {
            // 注册插件服务
            PluginsService.ConfigurePluginsServices(service);
        });
}

static Server BuildGrpcServer(IBot app)
    => new Server
    {
        Services =
        {
            Message.BindService(
                new MessageService(
					app.Context,
                    app.Context.ServiceProvider.GetRequiredService<ILoggerService>(),
                    app.Context.ServiceProvider.GetRequiredService<IMessageQueue>(),
                    app.Context.Configuration.GetSection("chat:token").Value
                ))
        },

        Ports =
        {
            new ServerPort(
                app.Context.Configuration.GetSection("chat:host").Value,
                int.Parse(app.Context.Configuration.GetSection("chat:port").Value!),
                ServerCredentials.Insecure)
        }
    };