namespace BankDashboard.Application.BankBalances.Dtos;

public sealed record BankBalanceListResponse(
    IReadOnlyList<BankBalanceDto> Items,
    int TotalCount,
    BankBalanceSummaryDto Summary);
