using RAG_BOT.Repository;
using System.Collections.Concurrent;
using WebhookAggergator.Models;
using WebhookAggergator.Services;

public class MessageAggregator : IMessageAggregator, IDisposable, IHostedService
{
    private readonly ILogger<MessageAggregator> _logger;
    private readonly IConfiguration _configuration;
    private readonly RedisService _RedisService;
    private TimeSpan _WaitTime { get; set; }
    private ConcurrentDictionary<string, Timer> _UserTimers { get; set; } = new();
    public MessageAggregator(ILogger<MessageAggregator> logger, IConfiguration configuration, RedisService redisService)
    {
        _logger = logger;
        _configuration = configuration;
        _RedisService = redisService;
        _WaitTime = TimeSpan.FromSeconds(_configuration.GetValue<int>("ResponseWaitTime", 5));
    }
    public void Dispose() { foreach (Timer timer in _UserTimers.Values) { timer.Dispose(); } }
    public void ProcessMessages(string userId)
    {
        string[] messages = _RedisService.GetValues(userId);
        if (messages.Length > 0)
        {
            // Add message processing logic here
            // Add auto reply for full message logic here
            _logger.LogInformation($"Processing messages for user: {userId}: {System.Text.Json.JsonSerializer.Serialize(messages)}");
            _RedisService.RemoveValue(userId);
        }
        if (_UserTimers.TryRemove(userId, out var timer)) { timer.Dispose(); }
    }
    public void QueueMessage(string userId, FbMessage message)
    {
        _logger.LogInformation($"Queueing message for user: {userId}, Message ID: {message.Id}");
        // Get or create queue for user
        //var queue = _MessageQueues.GetOrAdd(userId, (s) => new ConcurrentQueue<FbMessage>());
        //queue.Enqueue(message);
        _RedisService.AddValue(userId, message.Text);
        var timer = _UserTimers.GetOrAdd(userId, (t) => new Timer(new TimerCallback((_) => ProcessMessages(userId)), null, _WaitTime, Timeout.InfiniteTimeSpan));
        timer.Change(_WaitTime, Timeout.InfiniteTimeSpan); // Set the timer to trigger after the wait time
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MessageAggregator starting.");
        foreach (string userId in _RedisService.GetAllKeys())
        {
            _UserTimers.GetOrAdd(userId, (t) => new Timer(new TimerCallback((_) => ProcessMessages(userId)), null, _WaitTime, Timeout.InfiniteTimeSpan));
        }
        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MessageAggregator stopping.");
        foreach (var timer in _UserTimers.Values) { timer.Dispose(); }
        _RedisService.ResetDb();
        return Task.CompletedTask;
    }
}
