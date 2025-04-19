using Microsoft.EntityFrameworkCore;
using RAG_BOT.Repository;
using WebhookAggergator.Models;
using WebhookAggergator.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();



// Add services to the container.
builder.Services.AddHttpClient();

builder.Services.AddSingleton<RedisService>();





builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new()
    {
        LocalCacheExpiration = TimeSpan.FromMinutes(5),
    };
});

builder.AddKeyedRedisClient(name: "cache");
builder.AddKeyedRedisClient(name: "aggregation");
builder.AddRedisDistributedCache(connectionName :"cache");


builder.Services.AddSingleton<IMessageAggregator, MessageAggregator>();
builder.Services.AddHostedService<MessageAggregator>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();




app.MapPost("/ReceiveMessage", (MessengerWebHookModel request, IMessageAggregator aggregator) =>
{
    foreach (var entry in request.Entry)
    {
        foreach (var messagingEvent in entry.Messaging)
        {
            if (messagingEvent.Message != null)
            {
                // Queue the message
                aggregator.QueueMessage(messagingEvent.Sender.Id, messagingEvent.Message);
            }
        }
    }
    return Results.Ok();
});


app.Run();

