using BankDashboard.Infrastructure.Options;
using BankDashboard.Infrastructure.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BankDashboard.Application.Tests.BankBalances;

public sealed class JsonBankBalanceReadRepositoryTests
{
    [Fact]
    public async Task ListAsync_LoadsAllDemoBalancesFromJson()
    {
        var repository = new JsonBankBalanceReadRepository(
            Options.Create(new DemoDataOptions { FilePath = ResolveDemoDataPath() }),
            NullLogger<JsonBankBalanceReadRepository>.Instance);

        var balances = await repository.ListAsync(CancellationToken.None);

        Assert.Equal(5000, balances.Count);
        Assert.Equal(1, balances[0].Id);
        Assert.Equal(new DateOnly(2025, 1, 8), balances[0].Date);
        Assert.Equal("דיסקונט", balances[0].BankName);
        Assert.Equal("USD", balances[0].Currency);
    }

    private static string ResolveDemoDataPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "bank_balances_demo_5000.json");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not locate bank_balances_demo_5000.json from test output path.");
    }
}
