using System.Text;
using BankingSystemProject.Application.Commands;
using BankingSystemProject.Application.ViewModels;
using BankingSystemProject.Common.Services;
using BankingSystemProject.Domain.Models;
using BankingSystemProject.Infrastructure.Services;
using BankingSystemProject.Persistence.Data;
using BankingSystemProject.Persistence.Services.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace BankingSystemProject.Application.Handlers;

public class CreateRecurrentTransactionHandler : IRequestHandler<CreateRecurrentTransaction, RecurrenttransactionViewModel>
{
    private readonly DbContextFactory _dbContextFactory;
    private readonly ITenantService _tenantService;
    private readonly BankingSystemContext _context;
    private readonly CalculatNextTransactionDate _calculat;
    private readonly RabbitMqService _rabbitMqService;
    
    public CreateRecurrentTransactionHandler(DbContextFactory dbContextFactory, ITenantService tenantService, BankingSystemContext context, CalculatNextTransactionDate calculat, RabbitMqService rabbitMqService)
    {
        _dbContextFactory = dbContextFactory;
        _context = context;
        _tenantService = tenantService;
        _calculat = calculat;
        _rabbitMqService = rabbitMqService;
    }

    public async Task<RecurrenttransactionViewModel> Handle(CreateRecurrentTransaction request, CancellationToken cancellationToken)
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

        var user = context.Users
            .FirstOrDefault(u => u.Username == request.CustomerUsername);
        
        if (user == null)
        {
            throw new Exception("Customer not found on this branch.");
        }

        var account = await context.Accounts
            .FirstOrDefaultAsync(a => a.AccountId == request.AccountId && a.UserId == user.UserId, cancellationToken);

        if (account == null)
        {
            throw new Exception("Account not found for this customer on this branch");
        }

        // Check for withdrawal and ensure sufficient balance
        if (request.TransactionType == "Withdrawal" && account.Balance < request.Amount)
        {
            throw new Exception("Insufficient balance for withdrawal.");
        }
        
        // Create the recurrent transaction
        var recurrenttransaction = new Recurrenttransaction
        {
            Accountid = request.AccountId,
            Amount = request.Amount,
            Transactiontype = request.TransactionType,
            Createdat = DateTime.Now,
            Frequency = request.Frequency,
            Nexttransactiondate = _calculat.CalculateNextTransactionDate(request.Frequency, DateTime.Now)
        };

        context.Recurrenttransactions.Add(recurrenttransaction);
        await context.SaveChangesAsync(cancellationToken);
        
        _tenantService.SetSchema(defaultSchema);
        
        // Publish message to RabbitMQ
        var channel = _rabbitMqService.GetChannel();
        if (channel == null)
        {
            throw new Exception("RabbitMQ channel is not initialized.");
        }

        var message = JsonConvert.SerializeObject(new
        {
            Accountid = recurrenttransaction.Accountid,
            Amount = recurrenttransaction.Amount,
            Transactiontype = recurrenttransaction.Transactiontype,
            Createdat = recurrenttransaction.Createdat,
            Frequency = recurrenttransaction.Frequency,
            Nexttransactiondate = recurrenttransaction.Nexttransactiondate,
            Recurrenttransactionid = recurrenttransaction.Recurrenttransactionid,
            Branchid = request.BranchId
        });

        var body = Encoding.UTF8.GetBytes(message);
        channel.BasicPublish(exchange: "",
            routingKey: "recurrent_transactions",
            basicProperties: null,
            body: body);

        return new RecurrenttransactionViewModel
        {
            Recurrenttransactionid = recurrenttransaction.Recurrenttransactionid,
            Accountid = recurrenttransaction.Accountid,
            Amount = recurrenttransaction.Amount,
            Transactiontype = recurrenttransaction.Transactiontype,
            Createdat = recurrenttransaction.Createdat,
            Frequency = recurrenttransaction.Frequency,
            Nexttransactiondate = recurrenttransaction.Nexttransactiondate
        };
    }
}