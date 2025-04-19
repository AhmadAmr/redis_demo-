using StackExchange.Redis;

namespace RAG_BOT.Repository
{
    public class RedisService
    {
        private readonly IDatabase _db;
        private readonly ILogger<RedisService> _logger;

        public RedisService(
            [FromKeyedServices("aggregation")] IConnectionMultiplexer redis,
            ILogger<RedisService> logger)
        {
            _db = redis.GetDatabase();
            _logger = logger;
        }

        public void AddValue(string key, string value)
        {
            _db.ListRightPush(key, value);
            _logger.LogInformation($"Added value to Redis with key: {key}");
        }

        public string[] GetValues(string key)
        {
            var values = _db.ListRange(key).ToStringArray()!;
            _logger.LogInformation($"Retrieved values from Redis with key: {key}");
            return values;
        }

        public void RemoveValue(string key)
        {
            _logger.LogInformation($"Deleted value from Redis with key: {key}");
            _db.KeyDelete(key);
        }

        public void ResetDb()
        {
            _logger.LogWarning("Resetting the Redis database.");
            _db.Execute("FLUSHALL");
        }

        public string[] GetAllKeys()
        {
            var server = _db.Multiplexer.GetServer(_db.Multiplexer.Configuration);
            var keys = server.Keys(_db.Database).ToArray();
            _logger.LogInformation("Retrieved all keys from Redis.");
            return keys.Select(k => k.ToString()).ToArray();
        }
    }
}
