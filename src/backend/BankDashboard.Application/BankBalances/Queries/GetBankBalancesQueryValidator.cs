using FluentValidation;

namespace BankDashboard.Application.BankBalances.Queries;

public sealed class GetBankBalancesQueryValidator : AbstractValidator<GetBankBalancesQuery>
{
    private static readonly string[] AllowedSortFields =
    [
        "id",
        "date",
        "bankName",
        "accountNumber",
        "balanceType",
        "currency",
        "amount",
        "status"
    ];

    public GetBankBalancesQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 500);

        RuleFor(query => query.SortBy)
            .Must(BeAllowedSortField)
            .WithMessage($"'{{PropertyName}}' must be one of: {string.Join(", ", AllowedSortFields)}.");

        RuleFor(query => query.SortDirection)
            .Must(BeAllowedSortDirection)
            .WithMessage("'{PropertyName}' must be 'asc' or 'desc'.");

        RuleFor(query => query.MinAmount)
            .LessThanOrEqualTo(query => query.MaxAmount)
            .When(query => query.MinAmount is not null && query.MaxAmount is not null)
            .WithMessage("'Min Amount' cannot be greater than 'Max Amount'.");
    }

    private static bool BeAllowedSortField(string? sortBy) =>
        string.IsNullOrWhiteSpace(sortBy)
        || AllowedSortFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase);

    private static bool BeAllowedSortDirection(string? sortDirection) =>
        string.IsNullOrWhiteSpace(sortDirection)
        || string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase)
        || string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
}
