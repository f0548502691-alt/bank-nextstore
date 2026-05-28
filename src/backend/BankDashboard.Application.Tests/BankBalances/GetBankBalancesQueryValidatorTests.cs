using BankDashboard.Application.BankBalances.Queries;

namespace BankDashboard.Application.Tests.BankBalances;

public sealed class GetBankBalancesQueryValidatorTests
{
    private readonly GetBankBalancesQueryValidator _validator = new();

    [Fact]
    public void Validate_AcceptsValidQuery()
    {
        var result = _validator.Validate(CreateQuery());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0, 50, "date", "desc")]
    [InlineData(1, 0, "date", "desc")]
    [InlineData(1, 501, "date", "desc")]
    [InlineData(1, 50, "unsupported", "desc")]
    [InlineData(1, 50, "date", "sideways")]
    public void Validate_RejectsInvalidPagingAndSortingValues(
        int page,
        int pageSize,
        string sortBy,
        string sortDirection)
    {
        var result = _validator.Validate(CreateQuery(page: page, pageSize: pageSize, sortBy: sortBy, sortDirection: sortDirection));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsInvalidAmountRange()
    {
        var result = _validator.Validate(CreateQuery(minAmount: 200m, maxAmount: 100m));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetBankBalancesQuery.MinAmount));
    }

    private static GetBankBalancesQuery CreateQuery(
        decimal? minAmount = null,
        decimal? maxAmount = null,
        int page = 1,
        int pageSize = 50,
        string? sortBy = "date",
        string? sortDirection = "desc") =>
        new(
            Search: null,
            BankName: null,
            Currency: null,
            BalanceType: null,
            Status: null,
            MinAmount: minAmount,
            MaxAmount: maxAmount,
            Page: page,
            PageSize: pageSize,
            SortBy: sortBy,
            SortDirection: sortDirection);
}
