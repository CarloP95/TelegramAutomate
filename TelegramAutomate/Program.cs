using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramAutomate;
using TelegramAutomate.Abstract;
using TelegramAutomate.Commands;
using TelegramAutomate.Services;


IHost host = Host.CreateDefaultBuilder(args)
.ConfigureServices((context, services) =>
{
    // Register Bot configuration
    services.Configure<BotConfiguration>(context.Configuration.GetSection("BotConfiguration"));

    // Register named HttpClient to benefits from IHttpClientFactory and consume it with ITelegramBotClient typed client.
    // See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#typed-clients
    // and https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
    services.AddHttpClient("telegram_bot_client").RemoveAllLoggers()
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                BotConfiguration? botConfiguration = sp.GetService<IOptions<BotConfiguration>>()?.Value;
                ArgumentNullException.ThrowIfNull(botConfiguration);
                TelegramBotClientOptions options = new(botConfiguration.BotToken);
                return new TelegramBotClient(options, httpClient);
            });

    services.AddScoped<UpdateHandler>();
    services.AddScoped<ReceiverService>();
    services.AddTransient<AuthenticationService>();
    services.AddHostedService<PollingService>();



    #region commands

    services.AddSingleton<IBlobUploadService>(sp =>
    {
        var botConf = sp.GetService<IOptions<BotConfiguration>>()?.Value;
        return new BlobDriveCommand(true, "client_secret_apps.googleusercontent.com.json", "NAS");
    });

    services.AddSingleton<IUpdateCommand<Message>>(sp =>
    {
        return new PhotoCommand(sp.GetService<ITelegramBotClient>(), true);
    });
    services.AddSingleton<IUpdateCommand<Message>>(sp =>
    {
        return new InlineKeboardCommand(sp.GetService<ITelegramBotClient>(), true);
    });
    services.AddSingleton<IUpdateCommand<Message>>(sp =>
    {
        return new KeyboardCommand(sp.GetService<ITelegramBotClient>(), true);
    });
    services.AddSingleton<IUpdateCommand<Message>>(sp =>
    {
        return new RemoveKeyboardCommand(sp.GetService<ITelegramBotClient>(), true);
    });
    services.AddSingleton<IUpdateCommand<Message>>(sp =>
    {
        return new RequestCommand(sp.GetService<ITelegramBotClient>(), true);
    });
    services.AddSingleton<IUpdateCommand<Message>>(sp =>
    {
        return new InlineModeCommand(sp.GetService<ITelegramBotClient>(), true);
    });
    services.AddSingleton<IUpdateCommand<Message>>(sp =>
    {
        return new PollCommand(sp.GetService<ITelegramBotClient>(), true);
    });
    services.AddSingleton<IUpdateCommand<Message>>(sp =>
    {
        var botConf = sp.GetService<IOptions<BotConfiguration>>()?.Value;
        return new MakeTorrentCommand(sp.GetService<ITelegramBotClient>(), botConf.AllowTorrents, botConf.TorrentingForAdmin, botConf.TorrentPath, sp.GetService<AuthenticationService>());
    });
    services.AddSingleton<IUpdateCommand<Message>>(sp =>
    {
        var botConf = sp.GetService<IOptions<BotConfiguration>>()?.Value;
        var uploadServices = sp.GetServices<IBlobUploadService>();
        return new NASCommand(sp.GetService<ITelegramBotClient>(), sp.GetService<AuthenticationService>(), botConf.NASPath, uploadServices);
    });
    #endregion commands
})
.Build();

await host.RunAsync();
