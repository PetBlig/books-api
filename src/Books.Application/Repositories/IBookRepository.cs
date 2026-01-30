using Books.Domain;

namespace Books.Application.Repositories;

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken ct = default);
    Task<Book> CreateAsync(Book book, CancellationToken ct = default);
    Task<Book> UpdateAsync(Book book, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
