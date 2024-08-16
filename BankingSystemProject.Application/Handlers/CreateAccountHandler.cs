using AutoMapper;
using BankingSystemProject.Application.Commands;
using BankingSystemProject.Application.Services.Abstractions;
using BankingSystemProject.Domain.Models;
using BankingSystemProject.Persistence.Data;
using BankingSystemProject.Persistence.Services.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankingSystemProject.Application.Handlers;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccount>
{
    private readonly DbContextFactory _dbContextFactory;
    private readonly IGetAllAccountsService _getAllAccountsService;
    private readonly BankingSystemContext _context;
    private readonly IMapper _mapper;
    private readonly ITenantService _tenantService;
    
    public CreateAccountCommandHandler(DbContextFactory dbContextFactory, IGetAllAccountsService getAllAccountsService, BankingSystemContext context, IMapper mapper, ITenantService tenantService)
    {
        _dbContextFactory = dbContextFactory;
        _mapper = mapper;
        _tenantService = tenantService;
        _context = context;
        _getAllAccountsService = getAllAccountsService;
    }

    public async Task Handle(CreateAccount request, CancellationToken cancellationToken)
    {
        var customer = await _context.Users
            .Include(c => c.Accounts) //load accounts
            .FirstOrDefaultAsync(c => c.Username == request.CustomerUsername, cancellationToken);

        var toAddCustomer = await _context.Users
            .FirstOrDefaultAsync(c => c.Username == request.CustomerUsername, cancellationToken);
        
        if (customer == null)
        {
            throw new Exception("Customer not found.");
        }

        var allAccounts = _getAllAccountsService.GetAllAccounts(customer.UserId);
        Console.WriteLine(allAccounts.Result.Count);
        if ( allAccounts.Result.Count >= 5 )
        {
            throw new Exception("Customer has reached the maximum number of accounts.");
        }

        toAddCustomer.Accounts.Clear();
        
        var newAccount = new Account
        {
            UserId = customer.UserId,
            Balance = request.InitialBalance,
            CreatedAt = DateTime.Now,
        };
        
        customer.Accounts.Add(newAccount);
        
        // here
        var currentSchema = _tenantService.GetSchema();
        _tenantService.SetSchema(request.BranchId); 
        await using var context = _dbContextFactory.CreateDbContext();

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    
        try
        {
            // Check for existing user
            var existingUser = await context.Users
                .Where(u => u.Username == customer.Username)
                .FirstOrDefaultAsync(cancellationToken);
            
            if (existingUser == null)
            {
                context.Users.Add(toAddCustomer);
                context.Accounts.Add(newAccount);
                await context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                context.Accounts.Add(newAccount);
                await context.SaveChangesAsync(cancellationToken);
            }
        
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Log and handle other exceptions
            await transaction.RollbackAsync(cancellationToken);
            Console.WriteLine($"An error occurred: {ex.Message}");
            throw;
        }
        finally
        {
            _tenantService.SetSchema(currentSchema);
        }
    }
}