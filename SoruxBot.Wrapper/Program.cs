using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.DependencyInjection;
using SoruxBot.Kernel.Bot;
using SoruxBot.Kernel.Services.PushService;
using SoruxBot.SDK.Model.Message;
using SoruxBot.SDK.Plugins.Service;

var app = CreateDefaultBotBuilder(args).Build();

const string loggerName = "SoruxBot.Wrapper";
var logger = app.Context.ServiceProvider.GetRequiredService<ILoggerService>();

logger.Info(loggerName, $"SoruxBot Current Kernel Version: {app.Context.Configuration.GetSection("SDKVersion").Value}")
    .Info(loggerName, $"Running path: {app.Context.Configuration.GetSection("CurrentPath").Value}")
    .Info(loggerName, $"Development logger state: {app.Context.Configuration.GetSection("LoggerDebug").Value}")
    .Info(loggerName, $"SoruxBot running root path: {app.Context.Configuration.GetSection("storage:root").Value}");


var pushService = app.Context.ServiceProvider.GetRequiredService<IPushService>();
{
    pushService.RunInstance(
        (context) => {},
        (context) =>
        {
            // TODO 这里利用MessageContext，从Provider得到MessageId
            Console.WriteLine(context);
            var result = new MessageResult(
                "0",
                DateTime.Now
            );
            return result;
        });
}


logger.Info(loggerName, "SoruxBot has been initialized.");

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