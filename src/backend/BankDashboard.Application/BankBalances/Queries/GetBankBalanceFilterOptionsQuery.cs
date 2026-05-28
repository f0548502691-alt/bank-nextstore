using BankDashboard.Application.BankBalances.Dtos;
using MediatR;

namespace BankDashboard.Application.BankBalances.Queries;

public sealed record GetBankBalanceFilterOptionsQuery : IRequest<BankBalanceFilterOptionsDto>;
