using Anyware.OrdersAPI.Application.DTOs.Orders;
using Anyware.OrdersAPI.Application.Interfaces;
using Anyware.OrdersAPI.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace Anyware.OrdersAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

 
        [HttpPost]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequestDto request)
        {
            if (request == null)
                throw new ValidationException("Request body is required.");

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    );
                throw new ValidationException(errors);
            }

            var created = await _orderService.CreateOrderAsync(request, HttpContext.RequestAborted);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.OrderId },
                created
            );
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id, HttpContext.RequestAborted);

            if (order == null)
                throw new NotFoundException("Order", id);

            return Ok(order);
        }


        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderService.GetAllOrdersAsync(HttpContext.RequestAborted);
            return Ok(orders);
        }


        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _orderService.DeleteOrderAsync(id, HttpContext.RequestAborted);

            if (!deleted)
                throw new NotFoundException("Order", id);

            return NoContent();
        }
    }
}
