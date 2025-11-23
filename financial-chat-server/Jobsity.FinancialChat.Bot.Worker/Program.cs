using Jobsity.FinancialChat.Bot.Worker;
using Jobsity.FinancialChat.Bot.Worker.RabbitMqConfigurations;


Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<RabbitMqOptions>(
            context.Configuration.GetSection("RabbitMq"));

        services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();

        services.AddHttpClient("StockBot");

        services.AddHostedService<StockBotWorker>();
    })
    .Build()
    .Run();
