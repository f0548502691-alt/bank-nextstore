namespace BankDashboard.Application.BankBalances.Dtos;

public sealed record BankBalanceDto(
    int Id,
    DateOnly Date,
    string BankName,
    string AccountNumber,
    string BalanceType,
    string Currency,
    decimal Amount,
    string Status);
