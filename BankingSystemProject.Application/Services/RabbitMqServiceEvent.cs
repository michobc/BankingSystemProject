using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace BankingSystemProject.Application.Services;

public class RabbitMqServiceEvent
{
    private readonly IConfiguration _configuration;

    public RabbitMqServiceEvent(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IConnection GetConnection()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQ:HostName"],
            UserName = _configuration["RabbitMQ:UserName"],
            Password = _configuration["RabbitMQ:Password"]
        };
        return factory.CreateConnection();
    }
}