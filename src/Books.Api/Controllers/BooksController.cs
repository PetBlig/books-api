using Books.Application.Dtos;
using Books.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Books.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        ArgumentNullException.ThrowIfNull(bookService);
        _bookService = bookService;
    }

    /// <summary>Get all books</summary>
    [HttpGet]
    [Produces("application/json")]
    public async Task<IActionResult> GetAllAsync(CancellationToken ct)
    {
        var books = await _bookService.GetAllAsync(ct);
        return Ok(books);
    }

    /// <summary>Get book by id</summary>
    [HttpGet("{id}")]
    [Produces("application/json")]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken ct)
    {
        var book = await _bookService.GetByIdAsync(id, ct);
        return book is null ? NotFound() : Ok(book);
    }

    /// <summary>Create a new book</summary>
    [HttpPost]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateBookRequest request, CancellationToken ct)
    {
        try
        {
            var book = await _bookService.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = book.Id }, book);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Update a book</summary>
    [HttpPut("{id}")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateBookRequest request, CancellationToken ct)
    {
        try
        {
            var book = await _bookService.UpdateAsync(id, request, ct);
            return Ok(book);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Delete a book</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct)
    {
        try
        {
            await _bookService.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}
