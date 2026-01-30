using Books.T1.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace Books.T1.Tests.Repositories;

/// <summary>
/// T1-level sociable unit tests for InMemoryBookRepository.
/// Tests repository behavior: ID sequencing, timestamps, mutations, and state management.
/// </summary>
public class InMemoryBookRepositoryTests
{
    #region CreateAsync Tests

    [Fact]
    public async Task GIVEN_NewBook_WHEN_CreateCalled_THEN_AssignsIncrementingIds()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateFreshBookRepository();
        var book1 = new BookBuilder()
            .WithId(0)
            .WithTitle("Book 1")
            .Build();

        // Act
        var created1 = await repo.CreateAsync(book1, CancellationToken.None);

        // Assert
        created1.Id.Should().Be(1);
    }

    [Fact]
    public async Task GIVEN_MultipleCreates_WHEN_CalledSequentially_THEN_IdIncrementsCorrectly()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateFreshBookRepository();
        var book1 = new BookBuilder().WithTitle("Book 1").Build();
        var book2 = new BookBuilder().WithTitle("Book 2").Build();
        var book3 = new BookBuilder().WithTitle("Book 3").Build();

        // Act
        var created1 = await repo.CreateAsync(book1, CancellationToken.None);
        var created2 = await repo.CreateAsync(book2, CancellationToken.None);
        var created3 = await repo.CreateAsync(book3, CancellationToken.None);

        // Assert
        created1.Id.Should().Be(1);
        created2.Id.Should().Be(2);
        created3.Id.Should().Be(3);
    }

    [Fact]
    public async Task GIVEN_NewBook_WHEN_CreateCalled_THEN_SetsCreatedAtToUtcNow()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateFreshBookRepository();
        var beforeCreation = DateTime.UtcNow;
        var book = new BookBuilder()
            .WithCreatedAt(default) // Will be overwritten by repo
            .Build();

        // Act
        var created = await repo.CreateAsync(book, CancellationToken.None);
        var afterCreation = DateTime.UtcNow;

        // Assert
        created.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        created.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GIVEN_CreatedBook_WHEN_GetByIdCalled_THEN_ReturnsExactBook()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateFreshBookRepository();
        var book = new BookBuilder()
            .WithTitle("Clean Code")
            .WithAuthor("Robert C. Martin")
            .WithPrice(39.99m)
            .Build();

        var created = await repo.CreateAsync(book, CancellationToken.None);

        // Act
        var retrieved = await repo.GetByIdAsync(created.Id, CancellationToken.None);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved.Should().BeEquivalentTo(created);
        retrieved!.Title.Should().Be("Clean Code");
        retrieved.Author.Should().Be("Robert C. Martin");
        retrieved.Price.Should().Be(39.99m);
    }

    [Fact]
    public async Task GIVEN_InvalidId_WHEN_GetByIdCalled_THEN_ReturnsNull()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateFreshBookRepository();

        // Act
        var result = await repo.GetByIdAsync(999, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task GIVEN_ExistingBook_WHEN_UpdateCalled_THEN_MutatesExistingInstance()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateFreshBookRepository();
        var book = new BookBuilder()
            .WithTitle("Original Title")
            .WithPrice(19.99m)
            .Build();

        var created = await repo.CreateAsync(book, CancellationToken.None);
        var originalId = created.Id;

        var bookToUpdate = new BookBuilder()
            .WithId(originalId)
            .WithTitle("Updated Title")
            .WithPrice(29.99m)
            .Build();

        // Act
        var updated = await repo.UpdateAsync(bookToUpdate, CancellationToken.None);

        // Assert
        updated.Id.Should().Be(originalId);
        updated.Title.Should().Be("Updated Title");
        updated.Price.Should().Be(29.99m);

        // Verify mutation persisted
        var retrieved = await repo.GetByIdAsync(originalId, CancellationToken.None);
        retrieved!.Title.Should().Be("Updated Title");
        retrieved.Price.Should().Be(29.99m);
    }

    [Fact]
    public async Task GIVEN_NonExistentBook_WHEN_UpdateCalled_THEN_ThrowsInvalidOperationException()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateFreshBookRepository();
        var bookToUpdate = new BookBuilder()
            .WithId(999)
            .WithTitle("Non-existent")
            .Build();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.UpdateAsync(bookToUpdate, CancellationToken.None));
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task GIVEN_ExistingBook_WHEN_DeleteCalled_THEN_RemovesFromRepository()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateFreshBookRepository();
        var book = new BookBuilder().Build();
        var created = await repo.CreateAsync(book, CancellationToken.None);

        // Act
        await repo.DeleteAsync(created.Id, CancellationToken.None);

        // Assert
        var retrieved = await repo.GetByIdAsync(created.Id, CancellationToken.None);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task GIVEN_NonExistentBook_WHEN_DeleteCalled_THEN_ThrowsInvalidOperationException()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateFreshBookRepository();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.DeleteAsync(999, CancellationToken.None));
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GIVEN_EmptyRepository_WHEN_GetAllCalled_THEN_ReturnsEmptyCollection()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateFreshBookRepository();

        // Act
        var result = await repo.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GIVEN_MultipleBooks_WHEN_GetAllCalled_THEN_ReturnsAllBooks()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateFreshBookRepository();
        var book1 = new BookBuilder().WithTitle("Book 1").Build();
        var book2 = new BookBuilder().WithTitle("Book 2").Build();

        await repo.CreateAsync(book1, CancellationToken.None);
        await repo.CreateAsync(book2, CancellationToken.None);

        // Act
        var result = await repo.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Satisfy(
            b => b.Title == "Book 1",
            b => b.Title == "Book 2"
        );
    }

    #endregion
}
