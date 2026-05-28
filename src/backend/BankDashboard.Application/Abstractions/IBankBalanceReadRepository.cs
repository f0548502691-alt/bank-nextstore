using BankDashboard.Domain.BankBalances;

namespace BankDashboard.Application.Abstractions;

public interface IBankBalanceReadRepository
{
    Task<IReadOnlyList<BankBalance>> ListAsync(CancellationToken cancellationToken);
}
