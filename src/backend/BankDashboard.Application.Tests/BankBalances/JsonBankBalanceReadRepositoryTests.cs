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

    [Fact]
    public async Task ListAsync_RejectsDuplicateIds()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(
            filePath,
            """
            [
              {
                "id": 1,
                "date": "08/01/2025",
                "bankName": "דיסקונט",
                "accountNumber": "237167",
                "balanceType": "יתרת עו\"ש",
                "currency": "USD",
                "amount": 245571.18,
                "status": "פעיל"
              },
              {
                "id": 1,
                "date": "09/01/2025",
                "bankName": "פועלים",
                "accountNumber": "123456",
                "balanceType": "מניות",
                "currency": "ILS",
                "amount": 10,
                "status": "פעיל"
              }
            ]
            """);

        try
        {
            var repository = new JsonBankBalanceReadRepository(
                Options.Create(new DemoDataOptions { FilePath = filePath }),
                NullLogger<JsonBankBalanceReadRepository>.Instance);

            var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
                repository.ListAsync(CancellationToken.None));

            Assert.Contains("duplicate id", exception.Message);
        }
        finally
        {
            File.Delete(filePath);
        }
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
