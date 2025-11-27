using Anyware.OrdersAPI.Application.DTOs.Orders;
using Anyware.OrdersAPI.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        /// <summary>
        /// POST /api/orders
        /// Creates an order and returns 201 Created with Location header.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var created = await _orderService.CreateOrderAsync(request, HttpContext.RequestAborted);

                
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = created.OrderId },
                    created
                );
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Create order cancelled by client.");
                return StatusCode(499); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating an order.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// GET /api/orders/{id}
        /// Returns 200 OK with order or 404 NotFound if missing.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id, HttpContext.RequestAborted);
                if (order == null)
                    return NotFound();

                return Ok(order);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Get order by id cancelled by client. Id: {OrderId}", id);
                return StatusCode(499);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving order {OrderId}.", id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// GET /api/orders
        /// Lists all orders ordered by CreatedAt desc.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync(HttpContext.RequestAborted);
                return Ok(orders);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Get all orders cancelled by client.");
                return StatusCode(499);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all orders.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// DELETE /api/orders/{id}
        /// Deletes an order from DB and invalidates cache.
        /// Returns 204 NoContent on success or 404 if not found.
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await _orderService.DeleteOrderAsync(id, HttpContext.RequestAborted);
                if (!deleted)
                    return NotFound();

                return NoContent();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Delete order cancelled by client. Id: {OrderId}", id);
                return StatusCode(499);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting order {OrderId}.", id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
