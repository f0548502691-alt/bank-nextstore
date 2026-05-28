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
    private const int MaxPageSize = 500;

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
        [FromQuery] int pageSize = DefaultPageSize)
    {
        if (minAmount is not null && maxAmount is not null && minAmount > maxAmount)
        {
            ModelState.AddModelError(nameof(minAmount), "minAmount cannot be greater than maxAmount.");
        }

        if (page < 1)
        {
            ModelState.AddModelError(nameof(page), "page must be greater than or equal to 1.");
        }

        if (pageSize is < 1 or > MaxPageSize)
        {
            ModelState.AddModelError(nameof(pageSize), $"pageSize must be between 1 and {MaxPageSize}.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var response = await sender.Send(
            new GetBankBalancesQuery(search, bankName, currency, balanceType, status, minAmount, maxAmount, page, pageSize),
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
