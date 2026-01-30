using Books.Application.Dtos;

namespace Books.Application.Services;

public interface IOrderService
{
    Task<OrderDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken ct = default);
    Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken ct = default);
}
