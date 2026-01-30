using Books.Application.Services;
using Books.Infrastructure.Services;
using Books.T1.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace Books.T1.Tests.Services;

/// <summary>
/// T1-level sociable unit tests for BookService.
/// Tests service logic with real in-memory repository collaborators.
/// </summary>
public class BookServiceTests
{
    #region GetByIdAsync Tests

    [Fact]
    public async Task GIVEN_ExistingBook_WHEN_GetByIdCalled_THEN_ReturnsMappedDto()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var bookRequest = new CreateBookRequestBuilder()
            .WithTitle("Clean Code")
            .WithAuthor("Robert C. Martin")
            .WithPrice(39.99m)
            .WithStockQuantity(5)
            .Build();

        var service = new BookService(bookRepo);

        // Act
        var created = await service.CreateAsync(bookRequest, CancellationToken.None);
        var retrieved = await service.GetByIdAsync(created.Id, CancellationToken.None);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task GIVEN_NonExistentBookId_WHEN_GetByIdCalled_THEN_ReturnsNull()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var service = new BookService(bookRepo);

        // Act
        var result = await service.GetByIdAsync(999, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GIVEN_EmptyRepository_WHEN_GetAllCalled_THEN_ReturnsEmptyCollection()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var service = new BookService(bookRepo);

        // Act
        var result = await service.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GIVEN_MultipleBooks_WHEN_GetAllCalled_THEN_ReturnsAllMappedDtos()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var service = new BookService(bookRepo);

        var book1Request = new CreateBookRequestBuilder()
            .WithTitle("Clean Code")
            .WithAuthor("Robert C. Martin")
            .Build();

        var book2Request = new CreateBookRequestBuilder()
            .WithTitle("Design Patterns")
            .WithAuthor("Gang of Four")
            .Build();

        // Act
        await service.CreateAsync(book1Request, CancellationToken.None);
        await service.CreateAsync(book2Request, CancellationToken.None);
        var allBooks = await service.GetAllAsync(CancellationToken.None);

        // Assert
        allBooks.Should().HaveCount(2);
        allBooks.Should().Satisfy(
            b => b.Title == "Clean Code" && b.Author == "Robert C. Martin",
            b => b.Title == "Design Patterns" && b.Author == "Gang of Four"
        );
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task GIVEN_ValidCreateRequest_WHEN_CreateCalled_THEN_PersistsAndReturnsDto()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var service = new BookService(bookRepo);
        var request = new CreateBookRequestBuilder()
            .WithTitle("Clean Code")
            .WithAuthor("Robert C. Martin")
            .WithPrice(39.99m)
            .WithStockQuantity(10)
            .WithDescription("A Handbook of Agile Software Craftsmanship")
            .Build();

        // Act
        var created = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        created.Should().NotBeNull();
        created.Id.Should().BeGreaterThan(0);
        created.Title.Should().Be("Clean Code");
        created.Author.Should().Be("Robert C. Martin");
        created.Price.Should().Be(39.99m);
        created.StockQuantity.Should().Be(10);
        created.Description.Should().Be("A Handbook of Agile Software Craftsmanship");
        created.CreatedAt.Should().NotBe(default);

        // Verify persistence
        var retrieved = await service.GetByIdAsync(created.Id, CancellationToken.None);
        retrieved.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task GIVEN_InvalidTitle_WHEN_CreateCalled_THEN_ThrowsArgumentException()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var service = new BookService(bookRepo);
        var request = new CreateBookRequestBuilder()
            .WithTitle("   ")
            .Build();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task GIVEN_InvalidAuthor_WHEN_CreateCalled_THEN_ThrowsArgumentException()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var service = new BookService(bookRepo);
        var request = new CreateBookRequestBuilder()
            .WithAuthor("")
            .Build();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task GIVEN_PriceZeroOrNegative_WHEN_CreateCalled_THEN_ThrowsArgumentException()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var service = new BookService(bookRepo);

        // Act & Assert - Price = 0
        var requestZero = new CreateBookRequestBuilder().WithPrice(0m).Build();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(requestZero, CancellationToken.None));

        // Act & Assert - Price < 0
        var requestNegative = new CreateBookRequestBuilder().WithPrice(-10m).Build();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(requestNegative, CancellationToken.None));
    }

    [Fact]
    public async Task GIVEN_NegativeStock_WHEN_CreateCalled_THEN_ThrowsArgumentException()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var service = new BookService(bookRepo);
        var request = new CreateBookRequestBuilder()
            .WithStockQuantity(-1)
            .Build();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task GIVEN_NullRequest_WHEN_CreateCalled_THEN_ThrowsArgumentNullException()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var service = new BookService(bookRepo);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.CreateAsync(null!, CancellationToken.None));
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task GIVEN_ExistingBookAndValidUpdate_WHEN_UpdateCalled_THEN_UpdatesAndReturnsMappedDto()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var service = new BookService(bookRepo);

        var createRequest = new CreateBookRequestBuilder()
            .WithTitle("Original Title")
            .WithAuthor("Original Author")
            .WithPrice(19.99m)
            .Build();

        var created = await service.CreateAsync(createRequest, CancellationToken.None);

        var updateRequest = new UpdateBookRequestBuilder()
            .WithTitle("Updated Title")
            .WithAuthor("Updated Author")
            .WithPrice(29.99m)
            .Build();

        // Act
        var updated = await service.UpdateAsync(created.Id, updateRequest, CancellationToken.None);

        // Assert
        updated.Should().NotBeNull();
        updated.Title.Should().Be("Updated Title");
        updated.Author.Should().Be("Updated Author");
        updated.Price.Should().Be(29.99m);

        // Verify persistence
        var retrieved = await service.GetByIdAsync(created.Id, CancellationToken.None);
        retrieved.Should().BeEquivalentTo(updated);
    }

    [Fact]
    public async Task GIVEN_NonExistentBookId_WHEN_UpdateCalled_THEN_ThrowsInvalidOperationException()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var service = new BookService(bookRepo);
        var updateRequest = new UpdateBookRequestBuilder().Build();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateAsync(999, updateRequest, CancellationToken.None));
    }

    [Fact]
    public async Task GIVEN_ValidTitleUpdate_WHEN_UpdateCalled_THEN_OnlyTitleUpdated()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var service = new BookService(bookRepo);

        var createRequest = new CreateBookRequestBuilder()
            .WithTitle("Original")
            .WithAuthor("Author")
            .WithPrice(29.99m)
            .Build();

        var created = await service.CreateAsync(createRequest, CancellationToken.None);

        var updateRequest = new UpdateBookRequestBuilder()
            .WithTitle("Updated Title")
            .Build();

        // Act
        var updated = await service.UpdateAsync(created.Id, updateRequest, CancellationToken.None);

        // Assert
        updated.Title.Should().Be("Updated Title");
        updated.Author.Should().Be("Author"); // Unchanged
        updated.Price.Should().Be(29.99m); // Unchanged
    }

    [Fact]
    public async Task GIVEN_InvalidPriceInUpdate_WHEN_UpdateCalled_THEN_SilentlyIgnoresUpdate()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var service = new BookService(bookRepo);

        var originalPrice = 100m;
        var createRequest = new CreateBookRequestBuilder()
            .WithPrice(originalPrice)
            .Build();
        var created = await service.CreateAsync(createRequest, CancellationToken.None);

        // Act - Try to update with Price = 0 (should be ignored)
        var updateRequestZero = new UpdateBookRequestBuilder()
            .WithPrice(0m)
            .Build();
        var updatedZero = await service.UpdateAsync(created.Id, updateRequestZero, CancellationToken.None);

        // Assert - Price should remain unchanged
        updatedZero.Price.Should().Be(originalPrice);

        // Act - Try to update with negative price (should be ignored)
        var updateRequestNegative = new UpdateBookRequestBuilder()
            .WithPrice(-10m)
            .Build();
        var updatedNegative = await service.UpdateAsync(created.Id, updateRequestNegative, CancellationToken.None);

        // Assert - Price should still remain unchanged
        updatedNegative.Price.Should().Be(originalPrice);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task GIVEN_ExistingBook_WHEN_DeleteCalled_THEN_RemovesFromRepository()
    {
        // Arrange
        var bookRepo = TestRepositoryFactory.CreateFreshBookRepository();
        var service = new BookService(bookRepo);

        var createRequest = new CreateBookRequestBuilder().Build();
        var created = await service.CreateAsync(createRequest, CancellationToken.None);

        // Act
        await service.DeleteAsync(created.Id, CancellationToken.None);

        // Assert
        var retrieved = await service.GetByIdAsync(created.Id, CancellationToken.None);
        retrieved.Should().BeNull();
    }

    #endregion
}
