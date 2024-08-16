using BankingSystemProject.Persistence.Services;
using BankingSystemProject.Persistence.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BankingSystemProject.Persistence.Data;


public class DbContextFactory
{
    private readonly ITenantService _tenantService;

    public DbContextFactory(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    public BankingSystemContext CreateDbContext()
    {
        var schema = _tenantService.GetSchema();
        var options = new DbContextOptionsBuilder<BankingSystemContext>()
            .UseNpgsql($"Host=localhost;Database=bankingsystemdb;Username=postgres;Password=mypass03923367;Search Path={schema}")
            .Options;

        return new BankingSystemContext(options, _tenantService);
    }
}
