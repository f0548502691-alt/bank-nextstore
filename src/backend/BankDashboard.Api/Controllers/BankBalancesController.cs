using BankDashboard.Application.BankBalances.Dtos;
using BankDashboard.Application.BankBalances.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BankDashboard.Api.Controllers;

[ApiController]
[Route("api/bank-balances")]
public sealed class BankBalancesController(ISender sender) : ControllerBase
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 50;

    [HttpGet]
    [ProducesResponseType<BankBalanceListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BankBalanceListResponse>> GetBankBalances(
        [FromQuery] string? search,
        [FromQuery] string? bankName,
        [FromQuery] string? currency,
        [FromQuery] string? balanceType,
        [FromQuery] string? status,
        [FromQuery] decimal? minAmount,
        [FromQuery] decimal? maxAmount,
        CancellationToken cancellationToken,
        [FromQuery] int page = DefaultPage,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] string? sortBy = "date",
        [FromQuery] string? sortDirection = "desc")
    {
        var response = await sender.Send(
            new GetBankBalancesQuery(
                search,
                bankName,
                currency,
                balanceType,
                status,
                minAmount,
                maxAmount,
                page,
                pageSize,
                sortBy,
                sortDirection),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("filters")]
    [ProducesResponseType<BankBalanceFilterOptionsDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<BankBalanceFilterOptionsDto>> GetFilterOptions(CancellationToken cancellationToken)
    {
        var response = await sender.Send(new GetBankBalanceFilterOptionsQuery(), cancellationToken);
        return Ok(response);
    }
}
