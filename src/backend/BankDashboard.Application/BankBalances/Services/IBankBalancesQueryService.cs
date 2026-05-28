using BankDashboard.Application.BankBalances.Dtos;
using BankDashboard.Application.BankBalances.Queries;

namespace BankDashboard.Application.BankBalances.Services;

public interface IBankBalancesQueryService
{
    Task<BankBalanceListResponse> GetBankBalancesAsync(
        GetBankBalancesQuery query,
        CancellationToken cancellationToken);

    Task<BankBalanceFilterOptionsDto> GetFilterOptionsAsync(CancellationToken cancellationToken);
}
