using System.Globalization;
using System.Text.Json;
using BankDashboard.Application.Abstractions;
using BankDashboard.Domain.BankBalances;
using BankDashboard.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BankDashboard.Infrastructure.Repositories;

public sealed class JsonBankBalanceReadRepository : IBankBalanceReadRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly Lazy<Task<IReadOnlyList<BankBalance>>> _balances;
    private readonly ILogger<JsonBankBalanceReadRepository> _logger;
    private readonly DemoDataOptions _options;

    public JsonBankBalanceReadRepository(
        IOptions<DemoDataOptions> options,
        ILogger<JsonBankBalanceReadRepository> logger)
    {
        _options = options.Value;
        _logger = logger;
        _balances = new Lazy<Task<IReadOnlyList<BankBalance>>>(
            LoadBalancesAsync,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public Task<IReadOnlyList<BankBalance>> ListAsync(CancellationToken cancellationToken) =>
        _balances.Value.WaitAsync(cancellationToken);

    private async Task<IReadOnlyList<BankBalance>> LoadBalancesAsync()
    {
        var filePath = ResolveFilePath(_options.FilePath);
        _logger.LogInformation("Loading bank balances demo data from {FilePath}", filePath);

        await using var stream = File.OpenRead(filePath);
        var records = await JsonSerializer.DeserializeAsync<IReadOnlyList<JsonBankBalanceRecord>>(
            stream,
            SerializerOptions);

        if (records is null)
        {
            return [];
        }

        return records.Select(ToDomain).ToArray();
    }

    private static string ResolveFilePath(string configuredPath)
    {
        if (Path.IsPathRooted(configuredPath))
        {
            return configuredPath;
        }

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configuredPath));
    }

    private static BankBalance ToDomain(JsonBankBalanceRecord record) =>
        new(
            record.Id,
            ParseDate(record.Date, record.Id),
            Required(record.BankName, nameof(record.BankName), record.Id),
            Required(record.AccountNumber, nameof(record.AccountNumber), record.Id),
            Required(record.BalanceType, nameof(record.BalanceType), record.Id),
            Required(record.Currency, nameof(record.Currency), record.Id).ToUpperInvariant(),
            record.Amount,
            Required(record.Status, nameof(record.Status), record.Id));

    private static DateOnly ParseDate(string? value, int id)
    {
        if (DateOnly.TryParseExact(
                value,
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
        {
            return date;
        }

        throw new InvalidDataException($"Record {id} has an invalid date value: '{value}'. Expected dd/MM/yyyy.");
    }

    private static string Required(string? value, string fieldName, int id)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value.Trim();
        }

        throw new InvalidDataException($"Record {id} is missing required field '{fieldName}'.");
    }

    private sealed record JsonBankBalanceRecord(
        int Id,
        string? Date,
        string? BankName,
        string? AccountNumber,
        string? BalanceType,
        string? Currency,
        decimal Amount,
        string? Status);
}
