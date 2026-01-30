using Books.Application.Repositories;
using Books.Domain;

namespace Books.Infrastructure.Repositories;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = new();
    private int _nextId = 1;

    public Task<Order?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return Task.FromResult(_orders.FirstOrDefault(o => o.Id == id));
    }

    public Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<Order>>(_orders.AsReadOnly());
    }

    public Task<Order> CreateAsync(Order order, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(order);
        
        order.Id = _nextId++;
        order.CreatedAt = DateTime.UtcNow;
        _orders.Add(order);
        
        return Task.FromResult(order);
    }
}
