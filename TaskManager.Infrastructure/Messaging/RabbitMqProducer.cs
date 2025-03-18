using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TaskManager.Infrastructure.Helpers;

namespace TaskManager.Infrastructure.Messaging
{
    public class RabbitMqProducer :IMessageProducer
    {
        private IConnection _connection;
        private IChannel _channel;
        private readonly ILogger<RabbitMqProducer> _logger;
        //private readonly IConfiguration _configuration;
        //private const string ExchangeName = "task_events";
        //private const string RoutingKey = "task.created";
        private readonly RabbitMQConfig rabbitMQConfig;

        // Private constructor prevents direct instantiation.
        public RabbitMqProducer(IConfiguration configuration, ILogger<RabbitMqProducer> logger,IOptions<RabbitMQConfig> options)
        {
            rabbitMQConfig = options.Value;
            _logger = logger;

            try
            {
                var factory = new ConnectionFactory
                {
                    //HostName = configuration["RabbitMQ:Host"],
                    //UserName = configuration["RabbitMQ:Username"],
                    //Password = configuration["RabbitMQ:Password"]
                    HostName = rabbitMQConfig.Host,
                    UserName = rabbitMQConfig.Username,
                    Password = rabbitMQConfig.Password
                };

                _connection = factory.CreateConnectionAsync().Result;
                _channel = _connection.CreateChannelAsync().Result;

                _channel.ExchangeDeclareAsync(
                    exchange: rabbitMQConfig.ExchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);

                _logger.LogInformation("Connected to RabbitMQ");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to RabbitMQ");
                throw;
            }

        }

        public async Task PublishTaskCreated(TaskCreatedMessage message)
        {
            try
            {
                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                await _channel.BasicPublishAsync(
                    exchange: rabbitMQConfig.ExchangeName,
                    routingKey: rabbitMQConfig.RoutingKey,
                    body: body);

                _logger.LogInformation("Published task created message for TaskId: {TaskId}", message.TaskId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message to RabbitMQ");
            }
        }
    }
}
