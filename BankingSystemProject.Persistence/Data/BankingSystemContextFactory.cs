using BankingSystemProject.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BankingSystemProject.Persistence.Data;


public class BankingSystemContextFactory : IDesignTimeDbContextFactory<BankingSystemContext>
{
    public BankingSystemContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BankingSystemContext>();
        // Provide a connection string suitable for design-time
        optionsBuilder.UseNpgsql("Host=localhost;Database=bankingsystemdb;Username=postgres;Password=mypass03923367;Search Path=public");

        // You may need to mock or provide an instance of ITenantService
        var tenantService = new MockTenantService(); // Implement or mock this service

        return new BankingSystemContext(optionsBuilder.Options, tenantService);
    }
}
