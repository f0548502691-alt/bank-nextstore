namespace BankDashboard.Application.BankBalances.Dtos;

public sealed record BankBalanceFilterOptionsDto(
    IReadOnlyList<string> Banks,
    IReadOnlyList<string> Currencies,
    IReadOnlyList<string> BalanceTypes,
    IReadOnlyList<string> Statuses);
