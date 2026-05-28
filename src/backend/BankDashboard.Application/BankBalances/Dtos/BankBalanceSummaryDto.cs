namespace BankDashboard.Application.BankBalances.Dtos;

public sealed record BankBalanceSummaryDto(
    int TotalCount,
    int BankCount,
    DateOnly? LatestDate,
    IReadOnlyDictionary<string, decimal> TotalAmountByCurrency);
