using BankDashboard.Application.Abstractions;
using BankDashboard.Application.BankBalances.Queries;
using BankDashboard.Application.BankBalances.Services;
using BankDashboard.Domain.BankBalances;

namespace BankDashboard.Application.Tests.BankBalances;

public sealed class BankBalancesQueryServiceTests
{
    [Fact]
    public async Task Handle_FiltersBySearchAcrossKeyFields()
    {
        var service = CreateService(TestBalances);

        var response = await service.GetBankBalancesAsync(
            CreateQuery(search: "מניות"),
            CancellationToken.None);

        Assert.Single(response.Items);
        Assert.Equal(2, response.Items[0].Id);
    }

    [Fact]
    public async Task Handle_FiltersByAllSearchTermsAcrossDifferentFields()
    {
        var service = CreateService(
        [
            new(10, new DateOnly(2026, 2, 1), "לאומי", "111111", "אופציות", "ILS", 100m, "פעיל"),
            new(11, new DateOnly(2026, 2, 1), "לאומי", "222222", "מניות", "ILS", 100m, "פעיל"),
            new(12, new DateOnly(2026, 2, 1), "דיסקונט", "333333", "אופציות", "ILS", 100m, "פעיל")
        ]);

        var response = await service.GetBankBalancesAsync(
            CreateQuery(search: "לאומי אופציות"),
            CancellationToken.None);

        var item = Assert.Single(response.Items);
        Assert.Equal(10, item.Id);
    }

    [Fact]
    public async Task Handle_AppliesCurrencyStatusAndAmountRangeFilters()
    {
        var service = CreateService(TestBalances);

        var response = await service.GetBankBalancesAsync(
            CreateQuery(currency: "ILS", status: "פעיל", minAmount: 100m, maxAmount: 300m),
            CancellationToken.None);

        var item = Assert.Single(response.Items);
        Assert.Equal(2, item.Id);
        Assert.Equal(200m, response.Summary.TotalAmountByCurrency["ILS"]);
    }

    [Fact]
    public async Task Handle_SortsByDateDescendingThenIdDescending()
    {
        var service = CreateService(TestBalances);

        var response = await service.GetBankBalancesAsync(
            CreateQuery(),
            CancellationToken.None);

        Assert.Equal([3, 2, 1], response.Items.Select(item => item.Id));
    }

    [Fact]
    public async Task Handle_ReturnsRequestedPageAndPagingMetadata()
    {
        var service = CreateService(TestBalances);

        var response = await service.GetBankBalancesAsync(
            CreateQuery(page: 2, pageSize: 1),
            CancellationToken.None);

        var item = Assert.Single(response.Items);
        Assert.Equal(2, item.Id);
        Assert.Equal(3, response.TotalCount);
        Assert.Equal(2, response.Page);
        Assert.Equal(1, response.PageSize);
        Assert.Equal(3, response.TotalPages);
        Assert.True(response.HasPreviousPage);
        Assert.True(response.HasNextPage);
    }

    [Fact]
    public async Task Handle_ClampsPageWhenRequestedPageIsBeyondResults()
    {
        var service = CreateService(TestBalances);

        var response = await service.GetBankBalancesAsync(
            CreateQuery(page: 99, pageSize: 2),
            CancellationToken.None);

        var item = Assert.Single(response.Items);
        Assert.Equal(1, item.Id);
        Assert.Equal(2, response.Page);
        Assert.Equal(2, response.TotalPages);
        Assert.True(response.HasPreviousPage);
        Assert.False(response.HasNextPage);
    }

    [Fact]
    public async Task Handle_AppliesRequestedSort()
    {
        var service = CreateService(TestBalances);

        var response = await service.GetBankBalancesAsync(
            CreateQuery(sortBy: "amount", sortDirection: "asc"),
            CancellationToken.None);

        Assert.Equal([1, 2, 3], response.Items.Select(item => item.Id));
    }

    private static readonly IReadOnlyList<BankBalance> TestBalances =
    [
        new(1, new DateOnly(2025, 1, 8), "דיסקונט", "237167", "יתרת עו\"ש", "USD", 50m, "פעיל"),
        new(2, new DateOnly(2026, 1, 28), "דיסקונט", "422739", "מניות", "ILS", 200m, "פעיל"),
        new(3, new DateOnly(2026, 1, 28), "בינלאומי", "482229", "פיקדן קצוב - ריבית", "EUR", 500m, "חסום")
    ];

    private static GetBankBalancesQuery CreateQuery(
        string? search = null,
        string? bankName = null,
        string? currency = null,
        string? balanceType = null,
        string? status = null,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        int page = 1,
        int pageSize = 50,
        string? sortBy = "date",
        string? sortDirection = "desc") =>
        new(search, bankName, currency, balanceType, status, minAmount, maxAmount, page, pageSize, sortBy, sortDirection);

    private static BankBalancesQueryService CreateService(IReadOnlyList<BankBalance> balances) =>
        new(new InMemoryBankBalanceReadRepository(balances), new GetBankBalancesQueryValidator());

    private sealed class InMemoryBankBalanceReadRepository(IReadOnlyList<BankBalance> balances) : IBankBalanceReadRepository
    {
        public Task<IReadOnlyList<BankBalance>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult(balances);
    }
}
