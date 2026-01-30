using Books.Application.Dtos;
using Books.Application.Repositories;
using Books.Application.Services;
using Books.Domain;

namespace Books.Infrastructure.Services;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IBookRepository _bookRepository;

    public OrderService(IOrderRepository orderRepository, IBookRepository bookRepository)
    {
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(bookRepository);
        _orderRepository = orderRepository;
        _bookRepository = bookRepository;
    }

    public async Task<OrderDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, ct);
        return order is null ? null : MapToDto(order);
    }

    public async Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken ct = default)
    {
        var orders = await _orderRepository.GetAllAsync(ct);
        return orders.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(request.Quantity));

        var book = await _bookRepository.GetByIdAsync(request.BookId, ct)
            ?? throw new InvalidOperationException($"Book with id {request.BookId} not found");

        if (book.StockQuantity < request.Quantity)
            throw new InvalidOperationException($"Insufficient stock. Available: {book.StockQuantity}, Requested: {request.Quantity}");

        var totalPrice = book.Price * request.Quantity;

        var order = new Order
        {
            BookId = request.BookId,
            Quantity = request.Quantity,
            TotalPrice = totalPrice,
            Status = OrderStatus.Completed
        };

        // Decrement stock
        book.StockQuantity -= request.Quantity;
        await _bookRepository.UpdateAsync(book, ct);

        var created = await _orderRepository.CreateAsync(order, ct);
        return MapToDto(created);
    }

    private static OrderDto MapToDto(Order order) =>
        new(order.Id, order.BookId, order.Quantity, order.TotalPrice, order.Status, order.CreatedAt);
}
