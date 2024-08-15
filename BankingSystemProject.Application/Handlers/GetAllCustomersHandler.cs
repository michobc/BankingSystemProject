using AutoMapper;
using BankingSystemProject.Application.Commands;
using BankingSystemProject.Application.ViewModels;
using BankingSystemProject.Persistence.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankingSystemProject.Application.Handlers;

public class GetAllCustomersHandler: IRequestHandler<GetAllCustomers, List<CustomerViewModel>>
{
    private readonly BankingSystemContext _context;
    private readonly IMapper _mapper;

    public GetAllCustomersHandler(BankingSystemContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<CustomerViewModel>> Handle(GetAllCustomers request, CancellationToken cancellationToken)
    {
        var customers = await _context.Users
            .Where(u => u.Role == "Customer")
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