using AutoMapper;
using BankingSystemProject.Application.Commands;
using BankingSystemProject.Application.ViewModels;
using BankingSystemProject.Persistence.Data;
using BankingSystemProject.Persistence.Services.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankingSystemProject.Application.Handlers;

public class GetTransactionsHandler : IRequestHandler<GetTransactions, List<TransactionViewModel>>
{
    private readonly IMapper _mapper;
    private readonly ITenantService _tenantService;
    private readonly DbContextFactory _dbContextFactory;

    public GetTransactionsHandler(BankingSystemContext context, IMapper mapper, ITenantService tenantService, DbContextFactory dbContextFactory)
    {
        _mapper = mapper;
        _dbContextFactory = dbContextFactory;
        _tenantService = tenantService;
    }

    public async Task<List<TransactionViewModel>> Handle(GetTransactions request, CancellationToken cancellationToken)
    {
        var currentSchema = _tenantService.GetSchema();
        // Set the schema
        _tenantService.SetSchema(request.Branch);
        await using var context = _dbContextFactory.CreateDbContext();

        var user = context.Users
            .FirstOrDefault(u => u.Username == _tenantService.getUsername());

        if (user == null)
        {
            throw new Exception("No Account in this branch");
        }

        var account = context.Accounts
            .FirstOrDefault(a => a.UserId == user.UserId && a.AccountId == request.AccountId);

        if (account == null)
        {
            throw new Exception("No Account For you in this branch");
        }
        
        var transactions = await context.Transactions
            .Where(t => t.AccountId == request.AccountId)
            .ToListAsync(cancellationToken);

        if (transactions == null || !transactions.Any())
        {
            throw new Exception("No Transactions found for this account");
        }

        // Map entities to view models
        var tansactionViewModels = _mapper.Map<List<TransactionViewModel>>(transactions);

        return tansactionViewModels;
    }
}