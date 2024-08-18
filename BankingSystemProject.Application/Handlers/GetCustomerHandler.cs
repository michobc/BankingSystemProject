using AutoMapper;
using BankingSystemProject.Application.Commands;
using BankingSystemProject.Application.ViewModels;
using BankingSystemProject.Persistence.Data;
using BankingSystemProject.Persistence.Services.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BankingSystemProject.Application.Handlers;

public class GetCustomerHandler : IRequestHandler<GetCustomer, CustomerViewModel>
{
    private readonly BankingSystemContext _context;
    private readonly IMapper _mapper;
    private readonly DbContextFactory _dbContextFactory;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);
    private readonly ITenantService _tenantService;

    public GetCustomerHandler(BankingSystemContext context, IMapper mapper, IMemoryCache cache, ITenantService tenantService, DbContextFactory dbContextFactory)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
        _tenantService = tenantService;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<CustomerViewModel> Handle(GetCustomer request, CancellationToken cancellationToken)
    {
        var username = request.username;
        var currentSchema = _tenantService.GetSchema();
        
        // Switch to the current branch schema
        _tenantService.SetSchema(request.branch);

        // Create a new context for the branch
        await using var branchContext = _dbContextFactory.CreateDbContext();
        
        // if (!_cache.TryGetValue($"Customer_{username}", out CustomerViewModel cutomerViewModel))
        // {
            var customer = await branchContext.Users
                .Include(u => u.Accounts)
                .Where(u => u.Role == "Customer" && u.Username == username)
                .SingleOrDefaultAsync(cancellationToken);
            
            if (customer == null)
            {
                throw new Exception("No customer with this username found");
            }
            var customerView = _mapper.Map<CustomerViewModel>(customer);
            // Restore the original schema
            _tenantService.SetSchema(currentSchema);
            
            // _cache.Set($"Customer_{username}", customerView, _cacheDuration);
            // Console.WriteLine("added to cache");
            return customerView;
        // }
        // Console.WriteLine("From cache");
        // return cutomerViewModel;
    }
}