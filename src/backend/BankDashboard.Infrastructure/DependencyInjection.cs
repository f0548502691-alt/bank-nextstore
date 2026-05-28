using BankDashboard.Application.Abstractions;
using BankDashboard.Infrastructure.Options;
using BankDashboard.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankDashboard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DemoDataOptions>(configuration.GetSection(DemoDataOptions.SectionName));
        services.AddSingleton<IBankBalanceReadRepository, JsonBankBalanceReadRepository>();

        return services;
    }
}
