using BankingSystemProject.Application.Services.Abstractions;
using BankingSystemProject.Application.ViewModels;
using BankingSystemProject.Persistence.Data;
using BankingSystemProject.Persistence.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BankingSystemProject.Application.Services;

public class GetAllAccountsService : IGetAllAccountsService
{
    private readonly ITenantService _tenantService;
    private readonly DbContextFactory _dbContextFactory;
    
    public GetAllAccountsService(ITenantService tenantService, DbContextFactory dbContextFactory)
    {
        _tenantService = tenantService;
        _dbContextFactory = dbContextFactory;
    }
    
    public async Task<List<AccountViewModel>> GetAllAccounts(int userId)
    {
        var currentSchema = _tenantService.GetSchema();
        var allAccounts = new List<AccountViewModel>();

        try
        {
            // Set the schema to 'public' to access all branches
            _tenantService.SetSchema("public");

            // Create a new context to fetch branches
            await using var context = _dbContextFactory.CreateDbContext();
            var branches = await context.Branches.ToListAsync();

            // Loop through each branch
            foreach (var branch in branches)
            {
                // Switch to the current branch schema
                _tenantService.SetSchema(branch.BranchName);

                // Create a new context for the branch
                await using var branchContext = _dbContextFactory.CreateDbContext();

                // Fetch accounts for the user in the current branch schema
                var accounts = await branchContext.Accounts
                    .Where(a => a.UserId == userId)
                    .Select(a => new AccountViewModel
                    {
                        AccountId = a.AccountId,
                        UserId = a.UserId,
                        Balance = a.Balance,
                        CreatedAt = a.CreatedAt
                    })
                    .ToListAsync();

                allAccounts.AddRange(accounts);
            }
        }
        finally
        {
            // Restore the original schema
            _tenantService.SetSchema(currentSchema);
        }

        return allAccounts;
    }
}