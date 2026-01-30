using Books.T1.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace Books.T1.Tests.Repositories;

/// <summary>
/// T1-level sociable unit tests for InMemoryOrderRepository.
/// Tests order repository behavior: ID sequencing, timestamps, and retrieval.
/// </summary>
public class InMemoryOrderRepositoryTests
{
    #region CreateAsync Tests

    [Fact]
    public async Task GIVEN_NewOrder_WHEN_CreateCalled_THEN_AssignsIncrementingIds()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateFreshOrderRepository();
        var order1 = new OrderBuilder()
            .WithId(0)
            .WithBookId(1)
            .Build();

        // Act
        var created1 = await repo.CreateAsync(order1, CancellationToken.None);

        // Assert
        created1.Id.Should().Be(1);
    }

    [Fact]
    public async Task GIVEN_MultipleCreates_WHEN_CalledSequentially_THEN_IdIncrementsCorrectly()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateFreshOrderRepository();
        var order1 = new OrderBuilder().WithBookId(1).Build();
        var order2 = new OrderBuilder().WithBookId(1).Build();
        var order3 = new OrderBuilder().WithBookId(2).Build();

        // Act
        var created1 = await repo.CreateAsync(order1, CancellationToken.None);
        var created2 = await repo.CreateAsync(order2, CancellationToken.None);
        var created3 = await repo.CreateAsync(order3, CancellationToken.None);

        // Assert
        created1.Id.Should().Be(1);
        created2.Id.Should().Be(2);
        created3.Id.Should().Be(3);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GIVEN_CreatedOrders_WHEN_GetByIdCalled_THEN_ReturnsCorrectOrder()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateFreshOrderRepository();
        var order = new OrderBuilder()
            .WithBookId(1)
            .WithQuantity(5)
            .WithTotalPrice(99.95m)
            .Build();

        var created = await repo.CreateAsync(order, CancellationToken.None);

        // Act
        var retrieved = await repo.GetByIdAsync(created.Id, CancellationToken.None);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved.Should().BeEquivalentTo(created);
        retrieved!.BookId.Should().Be(1);
        retrieved.Quantity.Should().Be(5);
        retrieved.TotalPrice.Should().Be(99.95m);
    }

    [Fact]
    public async Task GIVEN_NonExistentOrderId_WHEN_GetByIdCalled_THEN_ReturnsNull()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateFreshOrderRepository();

        // Act
        var result = await repo.GetByIdAsync(999, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GIVEN_CreatedOrders_WHEN_GetAllCalled_THEN_ReturnsAllOrders()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateFreshOrderRepository();
        var order1 = new OrderBuilder().WithBookId(1).WithQuantity(2).Build();
        var order2 = new OrderBuilder().WithBookId(2).WithQuantity(3).Build();

        await repo.CreateAsync(order1, CancellationToken.None);
        await repo.CreateAsync(order2, CancellationToken.None);

        // Act
        var result = await repo.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Satisfy(
            o => o.BookId == 1 && o.Quantity == 2,
            o => o.BookId == 2 && o.Quantity == 3
        );
    }

    [Fact]
    public async Task GIVEN_EmptyRepository_WHEN_GetAllCalled_THEN_ReturnsEmptyCollection()
    {
        // Arrange
        var repo = TestRepositoryFactory.CreateFreshOrderRepository();

        // Act
        var result = await repo.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion
}
