namespace BankDashboard.Domain.BankBalances;

public sealed record BankBalance(
    int Id,
    DateOnly Date,
    string BankName,
    string AccountNumber,
    string BalanceType,
    string Currency,
    decimal Amount,
    string Status);
