using Books.Infrastructure.Repositories;

namespace Books.T1.Tests.Fixtures;

/// <summary>
/// Factory for creating fresh repository instances to ensure test isolation.
/// Each test gets its own repository state with no cross-test contamination.
/// </summary>
public static class TestRepositoryFactory
{
    public static InMemoryBookRepository CreateFreshBookRepository() =>
        new();

    public static InMemoryOrderRepository CreateFreshOrderRepository() =>
        new();
}
