using Books.Application.Dtos;
using Books.Domain;

namespace Books.Application.Services;

public interface IBookService
{
    Task<BookDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<BookDto>> GetAllAsync(CancellationToken ct = default);
    Task<BookDto> CreateAsync(CreateBookRequest request, CancellationToken ct = default);
    Task<BookDto> UpdateAsync(int id, UpdateBookRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
