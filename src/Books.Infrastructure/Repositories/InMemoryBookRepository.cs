using Books.Application.Repositories;
using Books.Domain;

namespace Books.Infrastructure.Repositories;

public sealed class InMemoryBookRepository : IBookRepository
{
    private readonly List<Book> _books = new();
    private int _nextId = 1;

    public Task<Book?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return Task.FromResult(_books.FirstOrDefault(b => b.Id == id));
    }

    public Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<Book>>(_books.AsReadOnly());
    }

    public Task<Book> CreateAsync(Book book, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(book);
        
        book.Id = _nextId++;
        book.CreatedAt = DateTime.UtcNow;
        _books.Add(book);
        
        return Task.FromResult(book);
    }

    public Task<Book> UpdateAsync(Book book, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(book);
        
        var existing = _books.FirstOrDefault(b => b.Id == book.Id)
            ?? throw new InvalidOperationException($"Book with id {book.Id} not found");

        existing.Title = book.Title;
        existing.Author = book.Author;
        existing.Price = book.Price;
        existing.StockQuantity = book.StockQuantity;
        existing.Description = book.Description;

        return Task.FromResult(existing);
    }

    public Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var book = _books.FirstOrDefault(b => b.Id == id)
            ?? throw new InvalidOperationException($"Book with id {id} not found");

        _books.Remove(book);
        return Task.CompletedTask;
    }
}
