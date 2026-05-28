using BankDashboard.Application.Abstractions;
using BankDashboard.Application.BankBalances.Dtos;
using BankDashboard.Domain.BankBalances;
using MediatR;

namespace BankDashboard.Application.BankBalances.Queries;

public sealed class GetBankBalancesQueryHandler(IBankBalanceReadRepository repository)
    : IRequestHandler<GetBankBalancesQuery, BankBalanceListResponse>
{
    public async Task<BankBalanceListResponse> Handle(GetBankBalancesQuery request, CancellationToken cancellationToken)
    {
        var balances = await repository.ListAsync(cancellationToken);

        var filtered = balances
            .Where(balance => MatchesSearch(balance, request.Search))
            .Where(balance => MatchesExact(balance.BankName, request.BankName))
            .Where(balance => MatchesExact(balance.Currency, request.Currency))
            .Where(balance => MatchesExact(balance.BalanceType, request.BalanceType))
            .Where(balance => MatchesExact(balance.Status, request.Status))
            .Where(balance => request.MinAmount is null || balance.Amount >= request.MinAmount.Value)
            .Where(balance => request.MaxAmount is null || balance.Amount <= request.MaxAmount.Value)
            .ToArray();

        var sorted = ApplySorting(filtered, request.SortBy, request.SortDirection).ToArray();
        var totalCount = filtered.Length;
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var page = totalPages == 0 ? 1 : Math.Min(request.Page, totalPages);
        var items = sorted
            .Skip((page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(ToDto)
            .ToArray();

        return new BankBalanceListResponse(
            items,
            totalCount,
            page,
            request.PageSize,
            totalPages,
            page > 1,
            totalPages > 0 && page < totalPages,
            BuildSummary(filtered));
    }

    private static bool MatchesSearch(BankBalance balance, string? search)
    {
        var term = Normalize(search);
        if (term is null)
        {
            return true;
        }

        return Contains(balance.BankName, term)
            || Contains(balance.AccountNumber, term)
            || Contains(balance.BalanceType, term)
            || Contains(balance.Currency, term)
            || Contains(balance.Status, term);
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
