using AutoMapper;
using BankingSystemProject.Application.Services.Abstractions;
using BankingSystemProject.Application.ViewModels;
using BankingSystemProject.Persistence.Data;
using BankingSystemProject.Persistence.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BankingSystemProject.Application.Services;

public class GetAllCustomersService : IGetAllCustomersService
{
    private readonly ITenantService _tenantService;
    private readonly DbContextFactory _dbContextFactory;
    private readonly IMapper _mapper;
    
    public GetAllCustomersService(ITenantService tenantService, DbContextFactory dbContextFactory, IMapper mapper)
    {
        _tenantService = tenantService;
        _mapper = mapper;
        _dbContextFactory = dbContextFactory;
    }
    
    public async Task<List<CustomerViewModel>> GetAllCustomers(string branch)
    {
        var currentSchema = _tenantService.GetSchema();
        var allCustomers = new List<CustomerViewModel>();

        try
        {
            // Switch to the current branch schema
            _tenantService.SetSchema(branch);

            // Create a new context for the branch
            await using var branchContext = _dbContextFactory.CreateDbContext();

            // Fetch accounts for the user in the current branch schema
            var customers = await branchContext.Users
                .Where(u => u.Role == "Customer" && u.BranchName == branch)
                .Select(a => new CustomerViewModel
                {
                    username = a.Username,
                    Branch = a.BranchName,
                    Accounts = _mapper.Map<List<AccountViewModel>>(a.Accounts),
                })
                .ToListAsync();

            allCustomers.AddRange(customers);

        }
        catch
        {
            throw new Exception("No Customers found in this branch");
        }
        finally
        {
            // Restore the original schema
            _tenantService.SetSchema(currentSchema);
        }

        return allCustomers;
    }
}