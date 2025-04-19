var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");
var aggregation = builder.AddRedis("aggregation");

builder.AddProject<Projects.RAG_BOT>("rag-bot")
       .WithReference(cache)
       .WithReference(aggregation)
       .WaitFor(aggregation)
       .WaitFor(cache);

builder.Build().Run();
