using Books.Domain;

namespace Books.Application.Dtos;

public record CreateOrderRequest(
    int BookId,
    int Quantity);

public record OrderDto(
    int Id,
    int BookId,
    int Quantity,
    decimal TotalPrice,
    OrderStatus Status,
    DateTime CreatedAt);
