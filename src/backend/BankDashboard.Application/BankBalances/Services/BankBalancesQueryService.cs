using BankDashboard.Application.Abstractions;
using BankDashboard.Application.BankBalances.Dtos;
using BankDashboard.Application.BankBalances.Queries;
using BankDashboard.Domain.BankBalances;
using FluentValidation;

namespace BankDashboard.Application.BankBalances.Services;

public sealed class BankBalancesQueryService(
    IBankBalanceReadRepository repository,
    IValidator<GetBankBalancesQuery> getBankBalancesQueryValidator) : IBankBalancesQueryService
{
    public async Task<BankBalanceListResponse> GetBankBalancesAsync(
        GetBankBalancesQuery query,
        CancellationToken cancellationToken)
    {
        await getBankBalancesQueryValidator.ValidateAndThrowAsync(query, cancellationToken);

        var balances = await repository.ListAsync(cancellationToken);

        var filtered = balances
            .Where(balance => MatchesSearch(balance, query.Search))
            .Where(balance => MatchesExact(balance.BankName, query.BankName))
            .Where(balance => MatchesExact(balance.Currency, query.Currency))
            .Where(balance => MatchesExact(balance.BalanceType, query.BalanceType))
            .Where(balance => MatchesExact(balance.Status, query.Status))
            .Where(balance => query.MinAmount is null || balance.Amount >= query.MinAmount.Value)
            .Where(balance => query.MaxAmount is null || balance.Amount <= query.MaxAmount.Value)
            .ToArray();

        var sorted = ApplySorting(filtered, query.SortBy, query.SortDirection).ToArray();
        var totalCount = filtered.Length;
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)query.PageSize);
        var page = totalPages == 0 ? 1 : Math.Min(query.Page, totalPages);
        var items = sorted
            .Skip((page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ToDto)
            .ToArray();

        return new BankBalanceListResponse(
            items,
            totalCount,
            page,
            query.PageSize,
            totalPages,
            page > 1,
            totalPages > 0 && page < totalPages,
            BuildSummary(filtered));
    }

    public async Task<BankBalanceFilterOptionsDto> GetFilterOptionsAsync(CancellationToken cancellationToken)
    {
        var balances = await repository.ListAsync(cancellationToken);

        return new BankBalanceFilterOptionsDto(
            SortDistinct(balances.Select(balance => balance.BankName)),
            SortDistinct(balances.Select(balance => balance.Currency)),
            SortDistinct(balances.Select(balance => balance.BalanceType)),
            SortDistinct(balances.Select(balance => balance.Status)));
    }

    private static bool MatchesSearch(BankBalance balance, string? search)
    {
        var terms = SplitSearchTerms(search);
        if (terms.Length == 0)
        {
            return true;
        }

        return terms.All(term =>
            Contains(balance.BankName, term)
            || Contains(balance.AccountNumber, term)
            || Contains(balance.BalanceType, term)
            || Contains(balance.Currency, term)
            || Contains(balance.Status, term));
    }

    private static bool MatchesExact(string value, string? filter)
    {
        var normalizedFilter = Normalize(filter);
        return normalizedFilter is null || string.Equals(value, normalizedFilter, StringComparison.OrdinalIgnoreCase);
    }

    private static bool Contains(string value, string term) =>
        value.Contains(term, StringComparison.OrdinalIgnoreCase);

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string[] SplitSearchTerms(string? search) =>
        Normalize(search)?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

    private static IOrderedEnumerable<BankBalance> ApplySorting(
        IEnumerable<BankBalance> balances,
        string? sortBy,
        string? sortDirection)
    {
        var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

        return Normalize(sortBy)?.ToLowerInvariant() switch
        {
            "id" => OrderBy(balance => balance.Id, descending),
            "date" => OrderBy(balance => balance.Date, descending),
            "bankname" => OrderBy(balance => balance.BankName, descending),
            "accountnumber" => OrderBy(balance => balance.AccountNumber, descending),
            "balancetype" => OrderBy(balance => balance.BalanceType, descending),
            "currency" => OrderBy(balance => balance.Currency, descending),
            "amount" => OrderBy(balance => balance.Amount, descending),
            "status" => OrderBy(balance => balance.Status, descending),
            _ => OrderBy(balance => balance.Date, descending: true)
        };

        IOrderedEnumerable<BankBalance> OrderBy<TKey>(Func<BankBalance, TKey> keySelector, bool descending) =>
            descending
                ? balances.OrderByDescending(keySelector).ThenByDescending(balance => balance.Id)
                : balances.OrderBy(keySelector).ThenBy(balance => balance.Id);
    }

    private static BankBalanceSummaryDto BuildSummary(IReadOnlyCollection<BankBalance> balances)
    {
        var totalsByCurrency = balances
            .GroupBy(balance => balance.Currency, StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(balance => balance.Amount),
                StringComparer.OrdinalIgnoreCase);

        return new BankBalanceSummaryDto(
            balances.Count,
            balances.Select(balance => balance.BankName).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            balances.Count == 0 ? null : balances.Max(balance => balance.Date),
            totalsByCurrency);
    }

    private static IReadOnlyList<string> SortDistinct(IEnumerable<string> values) =>
        values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static BankBalanceDto ToDto(BankBalance balance) =>
        new(
            balance.Id,
            balance.Date,
            balance.BankName,
            balance.AccountNumber,
            balance.BalanceType,
            balance.Currency,
            balance.Amount,
            balance.Status);
}
