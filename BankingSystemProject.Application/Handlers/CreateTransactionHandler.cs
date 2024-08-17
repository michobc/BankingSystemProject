using BankingSystemProject.Application.Commands;
using BankingSystemProject.Application.ViewModels;
using BankingSystemProject.Domain.Models;
using BankingSystemProject.Persistence.Data;
using BankingSystemProject.Persistence.Services.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankingSystemProject.Application.Handlers;

public class CreateTransactionHandler : IRequestHandler<CreateTransaction, TransactionViewModel>
{
    private readonly DbContextFactory _dbContextFactory;
    private readonly ITenantService _tenantService;
    private readonly BankingSystemContext _context;
    
    public CreateTransactionHandler(DbContextFactory dbContextFactory, ITenantService tenantService, BankingSystemContext context)
    {
        _dbContextFactory = dbContextFactory;
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<TransactionViewModel> Handle(CreateTransaction request, CancellationToken cancellationToken)
    {
        var defaultSchema = _tenantService.GetSchema();
        BankingSystemContext context;
        if (request.BranchId != defaultSchema)
        {
            _tenantService.SetSchema(request.BranchId);
            context = _dbContextFactory.CreateDbContext();
        }
        else
        {
            context = _context;
        }

        var user = context.Users.FirstOrDefault(u => u.Username == _tenantService.getUsername());
        
        if (user == null)
        {
            throw new Exception("No Account for you on this branch.");
        }

        var account = await context.Accounts
            .FirstOrDefaultAsync(a => a.AccountId == request.AccountId && a.UserId == user.UserId, cancellationToken);

        if (account == null)
        {
            throw new Exception("Account not found.");
        }

        // Check for withdrawal and ensure sufficient balance
        if (request.TransactionType == "Withdrawal" && account.Balance < request.Amount)
        {
            throw new Exception("Insufficient balance for withdrawal.");
        }

        // Adjust the balance
        if (request.TransactionType == "Deposit")
        {
            account.Balance += request.Amount;
        }
        else if (request.TransactionType == "Withdrawal")
        {
            account.Balance -= request.Amount;
        }

        // Create the transaction
        var transaction = new Transaction
        {
            AccountId = request.AccountId,
            Amount = request.Amount,
            TransactionType = request.TransactionType,
            CreatedAt = DateTime.Now
        };

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync(cancellationToken);
        
        _tenantService.SetSchema(defaultSchema);

        return new TransactionViewModel
        {
            TransactionId = transaction.TransactionId,
            AccountId = transaction.AccountId,
            Amount = transaction.Amount,
            TransactionType = transaction.TransactionType,
            CreatedAt = transaction.CreatedAt
        };
    }
}