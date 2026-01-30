using Books.Application.Dtos;
using Books.Domain;

namespace Books.T1.Tests.Fixtures;

/// <summary>
/// Fluent builders for creating domain and DTO objects in tests.
/// Enables readable test data setup without constructor noise.
/// </summary>

public class BookBuilder
{
    private int _id = 0;
    private string _title = "Test Book";
    private string _author = "Test Author";
    private decimal _price = 29.99m;
    private int _stockQuantity = 10;
    private string? _description = null;
    private DateTime _createdAt = DateTime.UtcNow;

    public BookBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public BookBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public BookBuilder WithAuthor(string author)
    {
        _author = author;
        return this;
    }

    public BookBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public BookBuilder WithStockQuantity(int quantity)
    {
        _stockQuantity = quantity;
        return this;
    }

    public BookBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public BookBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public Book Build() => new()
    {
        Id = _id,
        Title = _title,
        Author = _author,
        Price = _price,
        StockQuantity = _stockQuantity,
        Description = _description,
        CreatedAt = _createdAt
    };
}

public class OrderBuilder
{
    private int _id = 0;
    private int _bookId = 1;
    private int _quantity = 2;
    private decimal _totalPrice = 59.98m;
    private OrderStatus _status = OrderStatus.Completed;
    private DateTime _createdAt = DateTime.UtcNow;

    public OrderBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public OrderBuilder WithBookId(int bookId)
    {
        _bookId = bookId;
        return this;
    }

    public OrderBuilder WithQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    public OrderBuilder WithTotalPrice(decimal totalPrice)
    {
        _totalPrice = totalPrice;
        return this;
    }

    public OrderBuilder WithStatus(OrderStatus status)
    {
        _status = status;
        return this;
    }

    public OrderBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public Order Build() => new()
    {
        Id = _id,
        BookId = _bookId,
        Quantity = _quantity,
        TotalPrice = _totalPrice,
        Status = _status,
        CreatedAt = _createdAt
    };
}

public class CreateBookRequestBuilder
{
    private string _title = "Test Book";
    private string _author = "Test Author";
    private decimal _price = 29.99m;
    private int _stockQuantity = 10;
    private string? _description = null;

    public CreateBookRequestBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public CreateBookRequestBuilder WithAuthor(string author)
    {
        _author = author;
        return this;
    }

    public CreateBookRequestBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public CreateBookRequestBuilder WithStockQuantity(int quantity)
    {
        _stockQuantity = quantity;
        return this;
    }

    public CreateBookRequestBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public CreateBookRequest Build() =>
        new(_title, _author, _price, _stockQuantity, _description);
}

public class UpdateBookRequestBuilder
{
    private string? _title = null;
    private string? _author = null;
    private decimal? _price = null;
    private int? _stockQuantity = null;
    private string? _description = null;

    public UpdateBookRequestBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public UpdateBookRequestBuilder WithAuthor(string author)
    {
        _author = author;
        return this;
    }

    public UpdateBookRequestBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public UpdateBookRequestBuilder WithStockQuantity(int quantity)
    {
        _stockQuantity = quantity;
        return this;
    }

    public UpdateBookRequestBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public UpdateBookRequest Build() =>
        new(_title, _author, _price, _stockQuantity, _description);
}

public class CreateOrderRequestBuilder
{
    private int _bookId = 1;
    private int _quantity = 2;

    public CreateOrderRequestBuilder WithBookId(int bookId)
    {
        _bookId = bookId;
        return this;
    }

    public CreateOrderRequestBuilder WithQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    public CreateOrderRequest Build() =>
        new(_bookId, _quantity);
}
