using AutoMapper;
using BankingSystemProject.Application.Commands;
using BankingSystemProject.Application.Services.Abstractions;
using BankingSystemProject.Application.ViewModels;
using BankingSystemProject.Persistence.Data;
using BankingSystemProject.Persistence.Services.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankingSystemProject.Application.Handlers;

public class GetAllAccountsHandler: IRequestHandler<GetAllAccounts, List<AccountViewModel>>
{
    private readonly BankingSystemContext _context;
    private readonly IMapper _mapper;
    private readonly IGetAllAccountsService _getAllAccountsService;

    public GetAllAccountsHandler(BankingSystemContext context, IMapper mapper, IGetAllAccountsService getAllAccountsService)
    {
        _context = context;
        _mapper = mapper;
        _getAllAccountsService = getAllAccountsService;
    }

    public async Task<List<AccountViewModel>> Handle(GetAllAccounts request, CancellationToken cancellationToken)
    {
        var customer = await _context.Users
            .Include(c => c.Accounts)
            .Where(u => u.Role == "Customer" && u.Username == request.username)
            .SingleOrDefaultAsync(cancellationToken);

        if (customer == null)
        {
            throw new Exception("Customer not found in this branch");
        }

        var accounts = _getAllAccountsService.GetAllAccounts(customer.UserId);

        return accounts.Result;
    }
}