using Anyware.OrdersAPI.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Anyware.OrdersAPI.Infrastructure.Caching
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly ILogger<RedisCacheService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        };

        public RedisCacheService(
            IConnectionMultiplexer redis,
            ILogger<RedisCacheService> logger)
        {
            _redis = redis;
            _db = redis.GetDatabase();
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
            where T : class
        {
            try
            {
                if (!IsConnected())
                    return null;

                RedisValue value = await _db.StringGetAsync(key).ConfigureAwait(false);

                if (value.IsNullOrEmpty)
                {
                    _logger.LogDebug("Redis MISS for key {Key}", key);
                    return null;
                }

                _logger.LogDebug("Redis HIT for key {Key}", key);

                return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis error reading key {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
            where T : class
        {
            try
            {
                if (!IsConnected())
                    return;

                string json = JsonSerializer.Serialize(value, _jsonOptions);

                await _db.StringSetAsync(key, json, ttl).ConfigureAwait(false);

                _logger.LogDebug("Redis SET key {Key} with TTL {TTL}", key, ttl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis error setting key {Key}", key);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!IsConnected())
                    return;

                await _db.KeyDeleteAsync(key).ConfigureAwait(false);

                _logger.LogDebug("Redis REMOVE key {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis error removing key {Key}", key);
            }
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!IsConnected())
                    return false;

                return await _db.KeyExistsAsync(key).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis error checking key {Key}", key);
                return false;
            }
        }

        private bool IsConnected()
        {
            if (!_redis.IsConnected)
            {
                _logger.LogWarning("Redis is NOT connected.");
                return false;
            }

            return true;
        }
    }
}
