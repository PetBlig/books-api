using Books.Application.Dtos;
using Books.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Books.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        ArgumentNullException.ThrowIfNull(orderService);
        _orderService = orderService;
    }

    /// <summary>Get all orders</summary>
    [HttpGet]
    [Produces("application/json")]
    public async Task<IActionResult> GetAllAsync(CancellationToken ct)
    {
        var orders = await _orderService.GetAllAsync(ct);
        return Ok(orders);
    }

    /// <summary>Get order by id</summary>
    [HttpGet("{id}")]
    [Produces("application/json")]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken ct)
    {
        var order = await _orderService.GetByIdAsync(id, ct);
        return order is null ? NotFound() : Ok(order);
    }

    /// <summary>Create a new order</summary>
    [HttpPost]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        try
        {
            var order = await _orderService.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = order.Id }, order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
