namespace BankDashboard.Infrastructure.Options;

public sealed class DemoDataOptions
{
    public const string SectionName = "DemoData";

    public string FilePath { get; init; } = "data/bank_balances_demo_5000.json";
}
