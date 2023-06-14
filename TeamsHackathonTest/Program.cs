using AISuggestionAvish.Helper;
using AISuggestionAvish.Interfaces;
using Azure.AI.OpenAI;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.TeamsFx.Conversation;
using TeamsHackathonTest;
using TeamsHackathonTest.Bot;
using TeamsHackathonTest.Helper;
using TeamsHackathonTest.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient("WebClient", client => client.Timeout = TimeSpan.FromSeconds(600));
builder.Services.AddHttpContextAccessor();

//adding bot auth
builder.Configuration["MicrosoftAppType"] = "MultiTenant";
builder.Configuration["MicrosoftAppId"] = builder.Configuration.GetSection("BOT_ID")?.Value;
builder.Configuration["MicrosoftAppPassword"] = builder.Configuration.GetSection("BOT_PASSWORD")?.Value;
// Create the Bot Framework Authentication to be used with the Bot Adapter.
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
builder.Services.AddSingleton<IStorage, MemoryStorage>();
builder.Services.AddSingleton<UserState>();
builder.Services.AddSingleton<ConversationState>();
builder.Services.AddSingleton<CloudAdapter, AdapterWithErrorHandler>();
builder.Services.AddSingleton<IBotFrameworkHttpAdapter>(sp => sp.GetService<CloudAdapter>());
//builder.Services.AddSingleton<BotAdapter>(sp => sp.GetService<CloudAdapter>());
builder.Services.AddSingleton<IBot,InteractiveBot<CreativeDialogues>>();
builder.Services.AddSingleton(sp =>
{
    var options = new ConversationOptions()
    {
        Adapter = sp.GetService<CloudAdapter>(),
        Notification = new NotificationOptions
        {
            BotAppId = builder.Configuration["MicrosoftAppId"],
        },
    };

    return new ConversationBot(options);
});
builder.Services.AddSingleton<CreativeDialogues>();
// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
//builder.Services.AddTransient<IBot,InteractiveBot<CreativeDialogues>>();

//builder.Services.AddTransient<IBot,TeamsBot>();

builder.Services.AddSingleton<IOpenAiClient, TeamsHackathonTest.OpenAI.OpenAiClient>();
builder.Services.AddSingleton<IGraphAuthHepler, GraphAuthHepler>();
builder.Services.AddSingleton<IProactiveHelper, ProactiveHelper>();
builder.Services.AddSingleton<IFileHelper, FileHelper>();
builder.Services.AddSingleton<ConnectionHelper>();
builder.Services.AddSingleton<IDbHelper,DbHelper>();
builder.Services.AddTransient<IRunningTaskHelper,RunningTaskHelper>();
//builder.Services.AddSingleton<BackgroundWorker>();
//builder.Services.AddHostedService(provider => provider.GetRequiredService<BackgroundWorker>());

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
