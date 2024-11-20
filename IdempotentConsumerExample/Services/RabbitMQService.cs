using System.Text;
using IdempotentConsumerExample.Config;
using IdempotentConsumerExample.Db;
using IdempotentConsumerExample.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace IdempotentConsumerExample.Services;

public class RabbitMQService : BackgroundService
{
    internal readonly RabbitMqConfiguration _rabbitMqConfiguration;
    internal readonly IServiceScopeFactory _serviceScopeFactory;

    public RabbitMQService(IConfiguration config, IServiceScopeFactory serviceScopeFactory)
    {
        _rabbitMqConfiguration = config.GetSection("RabbitMQ").Get<RabbitMqConfiguration>()!;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public IConnection CreateChannel()
    {
        ConnectionFactory connectionFactory =
            new()
            {
                HostName = _rabbitMqConfiguration.HostName,
                UserName = _rabbitMqConfiguration.Username,
                Password = _rabbitMqConfiguration.Password,
                VirtualHost = "/",
            };

        connectionFactory.AutomaticRecoveryEnabled = true;
        connectionFactory.NetworkRecoveryInterval = TimeSpan.FromSeconds(5);
        connectionFactory.RequestedHeartbeat = TimeSpan.FromMinutes(5);
        connectionFactory.DispatchConsumersAsync = true;

        return connectionFactory.CreateConnection();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IConnection connection = CreateChannel();
        var channel = connection.CreateModel();
        foreach (Queues item in _rabbitMqConfiguration.QueueList!)
        {
            channel.ExchangeDeclare(item.Exchange, "direct");
            channel.QueueDeclare(
                queue: item.Queue,
                durable: true,
                exclusive: false,
                autoDelete: false
            );
            channel.QueueBind(item.Queue, item.Exchange, item.RoutingKey, null);
            channel.BasicQos(0, 1, false);
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());

                if (ea.RoutingKey.Equals("blogdirect"))
                {
                    var requestModel = JsonConvert.DeserializeObject<BlogRequestModel>(content)!;
                    var scope = _serviceScopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    bool messageExist = await context.Tbl_Messages.AnyAsync(x =>
                        x.MessageId == requestModel.MessageId
                    );
                    if (!messageExist)
                    {
                        // consume

                        var message = new Tbl_Messages()
                        {
                            MessageId = requestModel.MessageId,
                            IsProcessed = true,
                            ProcessedAt = DateTime.Now,
                        };
                        await context.Tbl_Messages.AddAsync(message);
                        await context.SaveChangesAsync();

                        return;
                    }
                }
            };

            channel.BasicConsume(item.Queue, false, consumer);
        }

        await Task.CompletedTask;
    }
}
