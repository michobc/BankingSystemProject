using AutoMapper;
using BankingSystemProject.Application.Commands;
using BankingSystemProject.Application.ViewModels;
using BankingSystemProject.Persistence.Data;
using BankingSystemProject.Persistence.Services.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankingSystemProject.Application.Handlers;

public class RollBackTransactionsHandler: IRequestHandler<RollBackTransactions, List<TransactionViewModel>>
{
    private readonly IMapper _mapper;
    private readonly ITenantService _tenantService;
    private readonly DbContextFactory _dbContextFactory;

    public RollBackTransactionsHandler(BankingSystemContext context, IMapper mapper, ITenantService tenantService, DbContextFactory dbContextFactory)
    {
        _mapper = mapper;
        _dbContextFactory = dbContextFactory;
        _tenantService = tenantService;
    }

    public async Task<List<TransactionViewModel>> Handle(RollBackTransactions request, CancellationToken cancellationToken)
    {
        var currentSchema = _tenantService.GetSchema();
        // Set the schema
        _tenantService.SetSchema(request.BranchId);
        await using var context = _dbContextFactory.CreateDbContext();

        var account = context.Accounts
            .FirstOrDefault(a => a.AccountId == request.AccountId);

        if (account == null)
        {
            throw new Exception("Account not found in this branch");
        }
        
        var transactions = await context.Transactions
            .Where(t => t.AccountId == request.AccountId && t.CreatedAt.Value.Date == request.Date.Date)
            .ToListAsync(cancellationToken);

        if (transactions == null || !transactions.Any())
        {
            throw new Exception("No Transactions found for this account in this date");
        }

        foreach (var transaction in transactions)
        {
            // Execute opposite transaction
            switch (transaction.TransactionType)
            {
                case "Deposit":
                    account.Balance -= transaction.Amount;
                    break;
                case "Withdrawal":
                    account.Balance += transaction.Amount;
                    break;
                default:
                    throw new Exception("Unknown transaction type");
            }

            // Remove the transaction from the database
            context.Transactions.Remove(transaction);
        }

        await context.SaveChangesAsync(cancellationToken);
        // Map entities to view models
        var tansactionViewModels = _mapper.Map<List<TransactionViewModel>>(transactions);

        return tansactionViewModels;
    }
}