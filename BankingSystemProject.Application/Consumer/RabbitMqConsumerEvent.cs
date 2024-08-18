using System.Text;
using BankingSystemProject.Application.Services;
using BankingSystemProject.Domain.Models;
using BankingSystemProject.Persistence.Data;
using BankingSystemProject.Persistence.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BankingSystemProject.Application.Consumer;

public class RabbitMqConsumerEvent : IHostedService
{
    private readonly ILogger<RabbitMqConsumerEvent> _logger;
    private readonly RabbitMqServiceEvent _rabbitMqService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private IModel _channel;
    private IConnection _connection;

    public RabbitMqConsumerEvent(ILogger<RabbitMqConsumerEvent> logger, RabbitMqServiceEvent rabbitMqService, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _rabbitMqService = rabbitMqService;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RabbitMqConsumerService is starting.");

        try
        {
            _connection = _rabbitMqService.GetConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "recurrent_transactions_exchange",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation("Received message: {Message}", message);

                try
                {
                    var data = JsonConvert.DeserializeObject<RecurrentTransactionEvent>(message);
                    await HandleMessageAsync(data);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling message");
                }
            };

            _channel.BasicConsume(queue: "recurrent_transactions_exchange",
                autoAck: true,
                consumer: consumer);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while starting RabbitMqConsumerService.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();
        return Task.CompletedTask;
    }

    private async Task HandleMessageAsync(RecurrentTransactionEvent data)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            Console.WriteLine("INNNNNNNNNNNNN");
            // Resolve the context factory and tenant service
            var contextFactory = scope.ServiceProvider.GetRequiredService<DbContextFactory>();
            var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();

            // Determine and set the schema
            var defaultSchema = tenantService.GetSchema();
            var context = contextFactory.CreateDbContext(); // Create a new DbContext instance
        
            if (data.BranchId != defaultSchema)
            {
                tenantService.SetSchema(data.BranchId);
                // Apply schema to the context dynamically
                context = contextFactory.CreateDbContext();
            }

            // Perform the database operations
            var recurrentTransaction = await context.Recurrenttransactions
                .FirstOrDefaultAsync(r => r.Recurrenttransactionid == data.RecurrentTransactionId);

            var account = await context.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == data.AccountId);
        
            if (recurrentTransaction != null && account != null)
            {
                // Update NextTransactionDate
                recurrentTransaction.Nexttransactiondate = data.NextTransactionDate;

                // Update account balance based on transaction type
                if (data.TransactionType == "Deposit")
                {
                    account.Balance += data.Amount;
                }
                else
                {
                    account.Balance -= data.Amount;
                }

                // Save changes to the database
                await context.SaveChangesAsync();
            }
        }
    }
}

public class RecurrentTransactionEvent
{
    public int RecurrentTransactionId { get; set; }
    public int AccountId { get; set; }
    public string BranchId { get; set; }
    public decimal Amount { get; set; }
    
    public string TransactionType { get; set; }
    public DateTime NextTransactionDate { get; set; }
}
