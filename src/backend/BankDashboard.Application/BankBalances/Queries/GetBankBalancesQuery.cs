using BankDashboard.Application.BankBalances.Dtos;
using MediatR;

namespace BankDashboard.Application.BankBalances.Queries;

public sealed record GetBankBalancesQuery(
    string? Search,
    string? BankName,
    string? Currency,
    string? BalanceType,
    string? Status,
    decimal? MinAmount,
    decimal? MaxAmount) : IRequest<BankBalanceListResponse>;
