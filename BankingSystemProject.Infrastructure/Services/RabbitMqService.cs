using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace BankingSystemProject.Infrastructure.Services;

public class RabbitMqService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqService(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"],
            UserName = configuration["RabbitMQ:UserName"],
            Password = configuration["RabbitMQ:Password"],
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: "recurrent_transactions",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public IModel GetChannel() => _channel;
}
