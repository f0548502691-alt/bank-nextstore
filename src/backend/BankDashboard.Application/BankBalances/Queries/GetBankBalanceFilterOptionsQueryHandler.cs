using BankDashboard.Application.Abstractions;
using BankDashboard.Application.BankBalances.Dtos;
using MediatR;

namespace BankDashboard.Application.BankBalances.Queries;

public sealed class GetBankBalanceFilterOptionsQueryHandler(IBankBalanceReadRepository repository)
    : IRequestHandler<GetBankBalanceFilterOptionsQuery, BankBalanceFilterOptionsDto>
{
    public async Task<BankBalanceFilterOptionsDto> Handle(
        GetBankBalanceFilterOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var balances = await repository.ListAsync(cancellationToken);

        return new BankBalanceFilterOptionsDto(
            SortDistinct(balances.Select(balance => balance.BankName)),
            SortDistinct(balances.Select(balance => balance.Currency)),
            SortDistinct(balances.Select(balance => balance.BalanceType)),
            SortDistinct(balances.Select(balance => balance.Status)));
    }

    private static IReadOnlyList<string> SortDistinct(IEnumerable<string> values) =>
        values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
