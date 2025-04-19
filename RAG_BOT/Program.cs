using Microsoft.EntityFrameworkCore;
using RAG_BOT.Repository;

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

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();



//call this to reproduce error point to call redis service
app.MapGet("/",  (RedisService redisService) =>
{
    var values = redisService.GetAllKeys();
    return Results.Ok(values);
});


app.Run();

