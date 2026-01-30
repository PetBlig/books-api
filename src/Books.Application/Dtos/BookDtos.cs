namespace Books.Application.Dtos;

public record CreateBookRequest(
    string Title,
    string Author,
    decimal Price,
    int StockQuantity,
    string? Description = null);

public record UpdateBookRequest(
    string? Title = null,
    string? Author = null,
    decimal? Price = null,
    int? StockQuantity = null,
    string? Description = null);

public record BookDto(
    int Id,
    string Title,
    string Author,
    decimal Price,
    int StockQuantity,
    string? Description,
    DateTime CreatedAt);
