using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace TaskManager.Infrastructure.Messaging
{
    public class RabbitMqProducer : IMessageProducer, IAsyncDisposable, IDisposable
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly ILogger<RabbitMqProducer> _logger;
        private readonly IConfiguration _configuration;
        private const string ExchangeName = "task_events";
        private const string RoutingKey = "task.created";

        // Private constructor prevents direct instantiation.
        private RabbitMqProducer(IConfiguration configuration, ILogger<RabbitMqProducer> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // Use this static method to asynchronously create and initialize an instance.
        public static async Task<RabbitMqProducer> CreateAsync(IConfiguration configuration, ILogger<RabbitMqProducer> logger)
        {
            var producer = new RabbitMqProducer(configuration, logger);
            await producer.InitializeAsync();
            return producer;
        }

        private async Task InitializeAsync()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _configuration["RabbitMQ:Host"],
                    UserName = _configuration["RabbitMQ:Username"],
                    Password = _configuration["RabbitMQ:Password"]
                };

                // Use the asynchronous connection creation method.
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                _channel.ExchangeDeclare(
                    exchange: ExchangeName,
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

        public void PublishTaskCreated(TaskCreatedMessage message)
        {
            try
            {
                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                _channel.BasicPublish(
                    exchange: ExchangeName,
                    routingKey: RoutingKey,
                    basicProperties: null,
                    body: body);

                _logger.LogInformation("Published task created message for TaskId: {TaskId}", message.TaskId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message to RabbitMQ");
            }
        }

        // Asynchronous disposal to properly release RabbitMQ resources.
        public async ValueTask DisposeAsync()
        {
            if (_channel != null)
            {
                await _channel.DisposeAsync();
            }
            if (_connection != null)
            {
                await _connection.DisposeAsync();
            }
            GC.SuppressFinalize(this);
        }

        // Synchronous disposal fallback.
        public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}
