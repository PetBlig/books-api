using Books.Application.Services;
using Books.Domain;
using Books.Infrastructure.Services;
using Books.T1.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace Books.T1.Tests.Services;

/// <summary>
/// T1-level sociable unit tests for OrderService.
/// Tests complex multi-repository orchestration with stock management.
/// </summary>
public class OrderServiceTests
{
    #region GetByIdAsync Tests

    [Fact]
    public async Task GIVEN_ExistingOrder_WHEN_GetByIdCalled_THEN_ReturnsMappedDto()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var orderRepo = TestRepositoryFactory.CreateFreshOrderRepository();

        var bookRequest = new CreateBookRequestBuilder().Build();
        var book = await bookRepo.CreateAsync(new()
        {
            Title = bookRequest.Title,
            Author = bookRequest.Author,
            Price = bookRequest.Price,
            StockQuantity = bookRequest.StockQuantity
        }, CancellationToken.None);

        var orderService = new OrderService(orderRepo, bookRepo);
        var orderRequest = new CreateOrderRequestBuilder()
            .WithBookId(book.Id)
            .WithQuantity(2)
            .Build();

        // Act
        var created = await orderService.CreateAsync(orderRequest, CancellationToken.None);
        var retrieved = await orderService.GetByIdAsync(created.Id, CancellationToken.None);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task GIVEN_NonExistentOrderId_WHEN_GetByIdCalled_THEN_ReturnsNull()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var orderRepo = TestRepositoryFactory.CreateFreshOrderRepository();
        var orderService = new OrderService(orderRepo, bookRepo);

        // Act
        var result = await orderService.GetByIdAsync(999, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GIVEN_EmptyOrderRepository_WHEN_GetAllCalled_THEN_ReturnsEmptyCollection()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var orderRepo = TestRepositoryFactory.CreateFreshOrderRepository();
        var orderService = new OrderService(orderRepo, bookRepo);

        // Act
        var result = await orderService.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GIVEN_MultipleOrders_WHEN_GetAllCalled_THEN_ReturnsAllMappedDtos()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var orderRepo = TestRepositoryFactory.CreateFreshOrderRepository();

        var bookRequest = new CreateBookRequestBuilder()
            .WithStockQuantity(100)
            .Build();
        var book = await bookRepo.CreateAsync(new()
        {
            Title = bookRequest.Title,
            Author = bookRequest.Author,
            Price = bookRequest.Price,
            StockQuantity = bookRequest.StockQuantity
        }, CancellationToken.None);

        var orderService = new OrderService(orderRepo, bookRepo);

        // Act
        var order1Request = new CreateOrderRequestBuilder()
            .WithBookId(book.Id)
            .WithQuantity(2)
            .Build();
        var order1 = await orderService.CreateAsync(order1Request, CancellationToken.None);

        var order2Request = new CreateOrderRequestBuilder()
            .WithBookId(book.Id)
            .WithQuantity(3)
            .Build();
        var order2 = await orderService.CreateAsync(order2Request, CancellationToken.None);

        var allOrders = await orderService.GetAllAsync(CancellationToken.None);

        // Assert
        allOrders.Should().HaveCount(2);
        allOrders.Should().Contain(o => o.Id == order1.Id);
        allOrders.Should().Contain(o => o.Id == order2.Id);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task GIVEN_ValidRequestAndSufficientStock_WHEN_CreateCalled_THEN_CreatesOrderAndDecrementsStock()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var orderRepo = TestRepositoryFactory.CreateFreshOrderRepository();

        var bookRequest = new CreateBookRequestBuilder()
            .WithPrice(29.99m)
            .WithStockQuantity(10)
            .Build();
        var book = await bookRepo.CreateAsync(new()
        {
            Title = bookRequest.Title,
            Author = bookRequest.Author,
            Price = bookRequest.Price,
            StockQuantity = bookRequest.StockQuantity
        }, CancellationToken.None);

        var orderService = new OrderService(orderRepo, bookRepo);
        var orderRequest = new CreateOrderRequestBuilder()
            .WithBookId(book.Id)
            .WithQuantity(3)
            .Build();

        // Act
        var created = await orderService.CreateAsync(orderRequest, CancellationToken.None);

        // Assert
        created.Should().NotBeNull();
        created.BookId.Should().Be(book.Id);
        created.Quantity.Should().Be(3);
        created.TotalPrice.Should().Be(89.97m); // 29.99 * 3
        created.Status.Should().Be(OrderStatus.Completed);

        // Verify stock decremented
        var updatedBook = await bookRepo.GetByIdAsync(book.Id, CancellationToken.None);
        updatedBook!.StockQuantity.Should().Be(7); // 10 - 3
    }

    [Fact]
    public async Task GIVEN_BookNotFound_WHEN_CreateCalled_THEN_ThrowsInvalidOperationException()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var orderRepo = TestRepositoryFactory.CreateFreshOrderRepository();
        var orderService = new OrderService(orderRepo, bookRepo);

        var orderRequest = new CreateOrderRequestBuilder()
            .WithBookId(999)
            .Build();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            orderService.CreateAsync(orderRequest, CancellationToken.None));

        // Verify no order created
        var allOrders = await orderService.GetAllAsync(CancellationToken.None);
        allOrders.Should().BeEmpty();
    }

    [Fact]
    public async Task GIVEN_InsufficientStock_WHEN_CreateCalled_THEN_ThrowsInvalidOperationException()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var orderRepo = TestRepositoryFactory.CreateFreshOrderRepository();

        var bookRequest = new CreateBookRequestBuilder()
            .WithStockQuantity(5)
            .Build();
        var book = await bookRepo.CreateAsync(new()
        {
            Title = bookRequest.Title,
            Author = bookRequest.Author,
            Price = bookRequest.Price,
            StockQuantity = bookRequest.StockQuantity
        }, CancellationToken.None);

        var orderService = new OrderService(orderRepo, bookRepo);
        var orderRequest = new CreateOrderRequestBuilder()
            .WithBookId(book.Id)
            .WithQuantity(10)
            .Build();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            orderService.CreateAsync(orderRequest, CancellationToken.None));

        // Verify stock unchanged
        var bookAfter = await bookRepo.GetByIdAsync(book.Id, CancellationToken.None);
        bookAfter!.StockQuantity.Should().Be(5);

        // Verify no order created
        var allOrders = await orderService.GetAllAsync(CancellationToken.None);
        allOrders.Should().BeEmpty();
    }

    [Fact]
    public async Task GIVEN_QuantityZeroOrNegative_WHEN_CreateCalled_THEN_ThrowsArgumentException()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var orderRepo = TestRepositoryFactory.CreateFreshOrderRepository();

        var bookRequest = new CreateBookRequestBuilder().Build();
        var book = await bookRepo.CreateAsync(new()
        {
            Title = bookRequest.Title,
            Author = bookRequest.Author,
            Price = bookRequest.Price,
            StockQuantity = bookRequest.StockQuantity
        }, CancellationToken.None);

        var orderService = new OrderService(orderRepo, bookRepo);

        // Act & Assert - Quantity = 0
        var requestZero = new CreateOrderRequestBuilder()
            .WithBookId(book.Id)
            .WithQuantity(0)
            .Build();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            orderService.CreateAsync(requestZero, CancellationToken.None));

        // Act & Assert - Quantity < 0
        var requestNegative = new CreateOrderRequestBuilder()
            .WithBookId(book.Id)
            .WithQuantity(-5)
            .Build();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            orderService.CreateAsync(requestNegative, CancellationToken.None));
    }

    [Fact]
    public async Task GIVEN_ExactStockQuantity_WHEN_CreateCalled_THEN_CreatesOrderAndStockBecomesZero()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var orderRepo = TestRepositoryFactory.CreateFreshOrderRepository();

        var bookRequest = new CreateBookRequestBuilder()
            .WithStockQuantity(5)
            .Build();
        var book = await bookRepo.CreateAsync(new()
        {
            Title = bookRequest.Title,
            Author = bookRequest.Author,
            Price = bookRequest.Price,
            StockQuantity = bookRequest.StockQuantity
        }, CancellationToken.None);

        var orderService = new OrderService(orderRepo, bookRepo);
        var orderRequest = new CreateOrderRequestBuilder()
            .WithBookId(book.Id)
            .WithQuantity(5)
            .Build();

        // Act
        var created = await orderService.CreateAsync(orderRequest, CancellationToken.None);

        // Assert
        created.Should().NotBeNull();
        var updatedBook = await bookRepo.GetByIdAsync(book.Id, CancellationToken.None);
        updatedBook!.StockQuantity.Should().Be(0); // 5 - 5
    }

    [Fact]
    public async Task GIVEN_MultipleOrdersForSameBook_WHEN_CreatedSequentially_THEN_StockDecrementsCorrectly()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var orderRepo = TestRepositoryFactory.CreateFreshOrderRepository();

        var bookRequest = new CreateBookRequestBuilder()
            .WithStockQuantity(100)
            .Build();
        var book = await bookRepo.CreateAsync(new()
        {
            Title = bookRequest.Title,
            Author = bookRequest.Author,
            Price = bookRequest.Price,
            StockQuantity = bookRequest.StockQuantity
        }, CancellationToken.None);

        var orderService = new OrderService(orderRepo, bookRepo);

        // Act - Order 1: quantity 20
        var order1Request = new CreateOrderRequestBuilder()
            .WithBookId(book.Id)
            .WithQuantity(20)
            .Build();
        await orderService.CreateAsync(order1Request, CancellationToken.None);

        var bookAfterOrder1 = await bookRepo.GetByIdAsync(book.Id, CancellationToken.None);
        var stockAfterOrder1 = bookAfterOrder1!.StockQuantity;

        // Act - Order 2: quantity 30
        var order2Request = new CreateOrderRequestBuilder()
            .WithBookId(book.Id)
            .WithQuantity(30)
            .Build();
        await orderService.CreateAsync(order2Request, CancellationToken.None);

        var bookAfterOrder2 = await bookRepo.GetByIdAsync(book.Id, CancellationToken.None);
        var stockAfterOrder2 = bookAfterOrder2!.StockQuantity;

        // Assert
        stockAfterOrder1.Should().Be(80); // 100 - 20
        stockAfterOrder2.Should().Be(50); // 80 - 30
    }

    [Fact]
    public async Task GIVEN_NullRequest_WHEN_CreateCalled_THEN_ThrowsArgumentNullException()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var orderRepo = TestRepositoryFactory.CreateFreshOrderRepository();
        var orderService = new OrderService(orderRepo, bookRepo);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            orderService.CreateAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task GIVEN_OrderStatus_WHEN_CreatedViaCreateAsync_THEN_AlwaysCompleted()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var orderRepo = TestRepositoryFactory.CreateFreshOrderRepository();

        var bookRequest = new CreateBookRequestBuilder().Build();
        var book = await bookRepo.CreateAsync(new()
        {
            Title = bookRequest.Title,
            Author = bookRequest.Author,
            Price = bookRequest.Price,
            StockQuantity = bookRequest.StockQuantity
        }, CancellationToken.None);

        var orderService = new OrderService(orderRepo, bookRepo);
        var orderRequest = new CreateOrderRequestBuilder()
            .WithBookId(book.Id)
            .Build();

        // Act
        var created = await orderService.CreateAsync(orderRequest, CancellationToken.None);

        // Assert
        created.Status.Should().Be(OrderStatus.Completed);
    }

    #endregion
}
