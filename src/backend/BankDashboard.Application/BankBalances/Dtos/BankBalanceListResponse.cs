namespace BankDashboard.Application.BankBalances.Dtos;

public sealed record BankBalanceListResponse(
    IReadOnlyList<BankBalanceDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage,
    BankBalanceSummaryDto Summary);
