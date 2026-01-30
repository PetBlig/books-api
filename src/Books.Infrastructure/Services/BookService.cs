using Books.Application.Dtos;
using Books.Application.Repositories;
using Books.Application.Services;
using Books.Domain;

namespace Books.Infrastructure.Services;

public sealed class BookService : IBookService
{
    private readonly IBookRepository _repository;

    public BookService(IBookRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public async Task<BookDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var book = await _repository.GetByIdAsync(id, ct);
        return book is null ? null : MapToDto(book);
    }

    public async Task<IReadOnlyList<BookDto>> GetAllAsync(CancellationToken ct = default)
    {
        var books = await _repository.GetAllAsync(ct);
        return books.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task<BookDto> CreateAsync(CreateBookRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateCreateRequest(request);

        var book = new Book
        {
            Title = request.Title,
            Author = request.Author,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            Description = request.Description
        };

        var created = await _repository.CreateAsync(book, ct);
        return MapToDto(created);
    }

    public async Task<BookDto> UpdateAsync(int id, UpdateBookRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existing = await _repository.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException($"Book with id {id} not found");

        if (!string.IsNullOrWhiteSpace(request.Title))
            existing.Title = request.Title;

        if (!string.IsNullOrWhiteSpace(request.Author))
            existing.Author = request.Author;

        if (request.Price.HasValue && request.Price > 0)
            existing.Price = request.Price.Value;

        if (request.StockQuantity.HasValue && request.StockQuantity >= 0)
            existing.StockQuantity = request.StockQuantity.Value;

        if (request.Description is not null)
            existing.Description = request.Description;

        var updated = await _repository.UpdateAsync(existing, ct);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await _repository.DeleteAsync(id, ct);
    }

    private static BookDto MapToDto(Book book) =>
        new(book.Id, book.Title, book.Author, book.Price, book.StockQuantity, book.Description, book.CreatedAt);

    private static void ValidateCreateRequest(CreateBookRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title cannot be empty", nameof(request.Title));

        if (string.IsNullOrWhiteSpace(request.Author))
            throw new ArgumentException("Author cannot be empty", nameof(request.Author));

        if (request.Price <= 0)
            throw new ArgumentException("Price must be greater than zero", nameof(request.Price));

        if (request.StockQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(request.StockQuantity));
    }
}
