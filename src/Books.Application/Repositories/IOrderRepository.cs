using Books.Domain;

namespace Books.Application.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default);
    Task<Order> CreateAsync(Order order, CancellationToken ct = default);
}
