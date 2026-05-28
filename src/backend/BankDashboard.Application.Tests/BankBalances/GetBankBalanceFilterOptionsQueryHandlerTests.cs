using BankDashboard.Application.Abstractions;
using BankDashboard.Application.BankBalances.Queries;
using BankDashboard.Application.BankBalances.Services;
using BankDashboard.Domain.BankBalances;

namespace BankDashboard.Application.Tests.BankBalances;

public sealed class BankBalancesQueryServiceFilterOptionsTests
{
    [Fact]
    public async Task GetFilterOptionsAsync_ReturnsDistinctSortedFilterValues()
    {
        var service = new BankBalancesQueryService(new InMemoryBankBalanceReadRepository(
        [
            new(1, new DateOnly(2025, 1, 8), "דיסקונט", "237167", "יתרת עו\"ש", "USD", 50m, "פעיל"),
            new(2, new DateOnly(2026, 1, 28), "דיסקונט", "422739", "מניות", "ILS", 200m, "פעיל"),
            new(3, new DateOnly(2026, 1, 28), "בינלאומי", "482229", "מניות", "EUR", 500m, "חסום")
        ]), new GetBankBalancesQueryValidator());

        var response = await service.GetFilterOptionsAsync(CancellationToken.None);

        Assert.Equal(["בינלאומי", "דיסקונט"], response.Banks);
        Assert.Equal(["EUR", "ILS", "USD"], response.Currencies);
        Assert.Equal(["יתרת עו\"ש", "מניות"], response.BalanceTypes);
        Assert.Equal(["חסום", "פעיל"], response.Statuses);
    }

    private sealed class InMemoryBankBalanceReadRepository(IReadOnlyList<BankBalance> balances) : IBankBalanceReadRepository
    {
        public Task<IReadOnlyList<BankBalance>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult(balances);
    }
}
