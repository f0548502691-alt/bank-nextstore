using BankDashboard.Application.Abstractions;
using BankDashboard.Application.BankBalances.Queries;
using BankDashboard.Domain.BankBalances;

namespace BankDashboard.Application.Tests.BankBalances;

public sealed class GetBankBalancesQueryHandlerTests
{
    [Fact]
    public async Task Handle_FiltersBySearchAcrossKeyFields()
    {
        var handler = new GetBankBalancesQueryHandler(new InMemoryBankBalanceReadRepository(TestBalances));

        var response = await handler.Handle(
            new GetBankBalancesQuery("מניות", null, null, null, null, null, null),
            CancellationToken.None);

        Assert.Single(response.Items);
        Assert.Equal(2, response.Items[0].Id);
    }

    [Fact]
    public async Task Handle_AppliesCurrencyStatusAndAmountRangeFilters()
    {
        var handler = new GetBankBalancesQueryHandler(new InMemoryBankBalanceReadRepository(TestBalances));

        var response = await handler.Handle(
            new GetBankBalancesQuery(null, null, "ILS", null, "פעיל", 100m, 300m),
            CancellationToken.None);

        var item = Assert.Single(response.Items);
        Assert.Equal(2, item.Id);
        Assert.Equal(200m, response.Summary.TotalAmountByCurrency["ILS"]);
    }

    [Fact]
    public async Task Handle_SortsByDateDescendingThenIdDescending()
    {
        var handler = new GetBankBalancesQueryHandler(new InMemoryBankBalanceReadRepository(TestBalances));

        var response = await handler.Handle(
            new GetBankBalancesQuery(null, null, null, null, null, null, null),
            CancellationToken.None);

        Assert.Equal([3, 2, 1], response.Items.Select(item => item.Id));
    }

    private static readonly IReadOnlyList<BankBalance> TestBalances =
    [
        new(1, new DateOnly(2025, 1, 8), "דיסקונט", "237167", "יתרת עו\"ש", "USD", 50m, "פעיל"),
        new(2, new DateOnly(2026, 1, 28), "דיסקונט", "422739", "מניות", "ILS", 200m, "פעיל"),
        new(3, new DateOnly(2026, 1, 28), "בינלאומי", "482229", "פיקדן קצוב - ריבית", "EUR", 500m, "חסום")
    ];

    private sealed class InMemoryBankBalanceReadRepository(IReadOnlyList<BankBalance> balances) : IBankBalanceReadRepository
    {
        public Task<IReadOnlyList<BankBalance>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult(balances);
    }
}
