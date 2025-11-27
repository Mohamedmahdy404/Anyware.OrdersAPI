using Anyware.OrdersAPI.Application.DTOs;
using Anyware.OrdersAPI.Application.DTOs.Orders;
using Anyware.OrdersAPI.Application.Interfaces;
using Anyware.OrdersAPI.Domain.Entities;
using Anyware.OrdersAPI.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Anyware.OrdersAPI.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IRedisCacheService _cache;
        private readonly ILogger<OrderService> _logger;

        private const string CACHE_KEY_PREFIX = "order:";
        private static readonly TimeSpan CacheTTL = TimeSpan.FromMinutes(5);

        public OrderService(
            IOrderRepository orderRepository,
            IRedisCacheService cache,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<OrderResponseDto> CreateOrderAsync(
            CreateOrderRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                CustomerName = request.CustomerName,
                Product = request.Product,
                Amount = request.Amount,
                CreatedAt = DateTime.UtcNow
            };

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            _logger.LogInformation("Order created {OrderId}", order.OrderId);

            //Cache
            await _cache.SetAsync(GetCacheKey(order.OrderId), order, CacheTTL, cancellationToken);

            return MapToResponse(order);
        }

        public async Task<OrderResponseDto?> GetOrderByIdAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            string key = GetCacheKey(orderId);

            //Try Redis Cache first
            var cachedOrder = await _cache.GetAsync<Order>(key, cancellationToken);
            if (cachedOrder != null)
            {
                _logger.LogInformation("Order {OrderId} retrieved from cache", orderId);
                return MapToResponse(cachedOrder);
            }

            //DB Fallback
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", orderId);
                return null;
            }

            _logger.LogInformation("Order {OrderId} retrieved from database", orderId);

            // Cache it
            await _cache.SetAsync(key, order, CacheTTL, cancellationToken);

            return MapToResponse(order);
        }

        public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync(
            CancellationToken cancellationToken = default)
        {
            var orders = await _orderRepository.GetAllAsync();

            return orders
                .OrderByDescending(o => o.CreatedAt)
                .Select(MapToResponse)
                .ToList();
        }

        public async Task<bool> DeleteOrderAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            // Check existence
            var existing = await _orderRepository.GetByIdAsync(orderId);
            if (existing == null)
            {
                _logger.LogWarning("Order {OrderId} not found for deletion", orderId);
                return false;
            }

            await _orderRepository.DeleteAsync(orderId);
            await _orderRepository.SaveChangesAsync();

            // Cache invalidation
            await _cache.RemoveAsync(GetCacheKey(orderId), cancellationToken);

            _logger.LogInformation("Order {OrderId} deleted", orderId);
            return true;
        }

        private static string GetCacheKey(Guid orderId)
            => $"{CACHE_KEY_PREFIX}{orderId}";

        private static OrderResponseDto MapToResponse(Order order)
        {
            return new OrderResponseDto
            {
                OrderId = order.OrderId,
                CustomerName = order.CustomerName,
                Product = order.Product,
                Amount = order.Amount,
                CreatedAt = order.CreatedAt
            };
        }
    }
}
