using AutoMapper;
using BankingSystemProject.Application.Commands;
using BankingSystemProject.Application.ViewModels;
using BankingSystemProject.Persistence.Data;
using BankingSystemProject.Persistence.Services.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankingSystemProject.Application.Handlers;

public class GetAllCustomersHandler: IRequestHandler<GetAllCustomers, List<CustomerViewModel>>
{
    private readonly BankingSystemContext _context;
    private readonly IMapper _mapper;
    private readonly ITenantService _tenantService;

    public GetAllCustomersHandler(BankingSystemContext context, IMapper mapper, ITenantService tenantService)
    {
        _context = context;
        _mapper = mapper;
        _tenantService = tenantService;
    }

    public async Task<List<CustomerViewModel>> Handle(GetAllCustomers request, CancellationToken cancellationToken)
    {
        var customers = await _context.Users
            .Include(c => c.Accounts)
            .Where(u => u.Role == "Customer" && u.BranchName == _tenantService.GetSchema())
            .ToListAsync(cancellationToken);

        if (customers == null || !customers.Any())
        {
            throw new Exception("No customers found in this branch");
        }

        // Map entities to view models
        var customerViewModels = _mapper.Map<List<CustomerViewModel>>(customers);

        return customerViewModels;
    }
}