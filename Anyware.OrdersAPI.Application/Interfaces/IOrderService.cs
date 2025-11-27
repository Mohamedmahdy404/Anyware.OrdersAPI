using Anyware.OrdersAPI.Application.DTOs.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anyware.OrdersAPI.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto request, CancellationToken cancellationToken = default);
        Task<OrderResponseDto?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
        Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync(CancellationToken cancellationToken = default);
        Task<bool> DeleteOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    }
}
