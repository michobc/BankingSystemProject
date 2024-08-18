using AutoMapper;
using BankingSystemProject.Application.Services.Abstractions;
using BankingSystemProject.Application.ViewModels;
using BankingSystemProject.Persistence.Data;
using BankingSystemProject.Persistence.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BankingSystemProject.Application.Services;

public class GetAllEmployeesService : IGetAllEmployeesService
{
    private readonly ITenantService _tenantService;
    private readonly DbContextFactory _dbContextFactory;
    private readonly IMapper _mapper;
    
    public GetAllEmployeesService(ITenantService tenantService, DbContextFactory dbContextFactory, IMapper mapper)
    {
        _tenantService = tenantService;
        _mapper = mapper;
        _dbContextFactory = dbContextFactory;
    }
    
    public async Task<List<EmployeeViewModel>> GetAllEmployees()
    {
        var currentSchema = _tenantService.GetSchema();
        var allEmployees = new List<EmployeeViewModel>();

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
                var employees = await branchContext.Users
                    .Where(a => a.Role == "Employee")
                    .Select(a => new EmployeeViewModel
                    {
                        username = a.Username,
                        Branch = a.BranchName
                    })
                    .ToListAsync();

                allEmployees.AddRange(employees);
            }
        }
        finally
        {
            // Restore the original schema
            _tenantService.SetSchema(currentSchema);
        }

        return allEmployees;
    }
    
    public async Task<EmployeeViewModel> GetMyEmployee(string username)
    {
        var currentSchema = _tenantService.GetSchema();
        var myEmployee = new EmployeeViewModel();

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
                var employee = branchContext.Users
                    .FirstOrDefault(a => a.Role == "Employee" && a.Username == username);

                if (employee != null)
                {
                    myEmployee = new EmployeeViewModel
                    {
                        username = employee.Username,
                        Branch = employee.BranchName
                    };
                    break;
                }
            }
        }
        finally
        {
            // Restore the original schema
            _tenantService.SetSchema(currentSchema);
        }

        return myEmployee;
    }
}