using BankDashboard.Application.BankBalances.Dtos;
using BankDashboard.Application.BankBalances.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BankDashboard.Api.Controllers;

[ApiController]
[Route("api/bank-balances")]
public sealed class BankBalancesController(ISender sender) : ControllerBase
{
    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "id",
        "date",
        "bankName",
        "accountNumber",
        "balanceType",
        "currency",
        "amount",
        "status"
    };

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
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] string? sortBy = "date",
        [FromQuery] string? sortDirection = "desc")
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

        if (!string.IsNullOrWhiteSpace(sortBy) && !AllowedSortFields.Contains(sortBy))
        {
            ModelState.AddModelError(nameof(sortBy), $"sortBy must be one of: {string.Join(", ", AllowedSortFields)}.");
        }

        if (!string.IsNullOrWhiteSpace(sortDirection)
            && !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(sortDirection), "sortDirection must be 'asc' or 'desc'.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

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
