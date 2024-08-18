using AutoMapper;
using BankingSystemProject.Application.Commands;
using BankingSystemProject.Application.Services.Abstractions;
using BankingSystemProject.Application.ViewModels;
using BankingSystemProject.Persistence.Data;
using BankingSystemProject.Persistence.Services.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankingSystemProject.Application.Handlers;

public class GetAllMyAccountsHandler: IRequestHandler<GetAllMyAccounts, List<AccountViewModel>>
{
    private readonly BankingSystemContext _context;
    private readonly IMapper _mapper;
    private readonly IGetAllAccountsService _getAllAccountsService;
    private readonly ITenantService _tenantService;

    public GetAllMyAccountsHandler(BankingSystemContext context, IMapper mapper, IGetAllAccountsService getAllAccountsService, ITenantService tenantService)
    {
        _context = context;
        _mapper = mapper;
        _tenantService = tenantService;
        _getAllAccountsService = getAllAccountsService;
    }

    public async Task<List<AccountViewModel>> Handle(GetAllMyAccounts request, CancellationToken cancellationToken)
    {
        var user = _context.Users
            .FirstOrDefault(u => u.Username == _tenantService.getUsername());
        if (user == null)
        {
            throw new Exception("You are not registered in DB");
        }
        var accounts = await _getAllAccountsService.GetAllAccounts(user.UserId);

        return accounts;
    }
}