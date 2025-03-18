using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TaskManager.Infrastructure.Caching;

namespace TaskManager.Infrastructure.Messaging
{
    public class RabbitMqConsumer : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<RabbitMqConsumer> _logger;
        private readonly string _exchangeName;
        private readonly string _queueName;
        private readonly string _routingKey;

        public RabbitMqConsumer(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<RabbitMqConsumer> logger
            //string exchangeName,
            //string queueName,
            //string routingKey)
            )

        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _exchangeName = "task_events";
            _queueName = "task_created_queue";
            _routingKey = "task.created";

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = configuration["RabbitMQ:Host"],
                    UserName = configuration["RabbitMQ:Username"],
                    Password = configuration["RabbitMQ:Password"]
                };

                _connection = factory.CreateConnectionAsync().Result;
                _channel = _connection.CreateChannelAsync().Result;

                _channel.ExchangeDeclareAsync(
                    exchange: _exchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);

                _channel.QueueDeclareAsync(
                    queue: _queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false);

                _channel.QueueBindAsync(
                    queue: _queueName,
                    exchange: _exchangeName,
                    routingKey: _routingKey);

                _logger.LogInformation("RabbitMQ consumer initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize RabbitMQ consumer");
                throw;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = JsonSerializer.Deserialize<TaskCreatedMessage>(Encoding.UTF8.GetString(body));

                    _logger.LogInformation("Received task created message for TaskId: {TaskId}", message.TaskId);

                    // Process the message
                    await ProcessTaskCreatedMessageAsync(message);

                    // Acknowledge the message
                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing RabbitMQ message");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            await _channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer);
        }

        private async Task ProcessTaskCreatedMessageAsync(TaskCreatedMessage message)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

            // Invalidate relevant cache entries
            await cacheService.RemoveAsync($"task:{message.TaskId}");
                await cacheService.RemoveAsync("tasks:all:*");
                await cacheService.RemoveAsync("tasks:status:Pending:*");

            if(message.IsNew)
            {
                _logger.LogInformation("Processed task created message for TaskId: {TaskId}", message.TaskId);
            }
            else
            {
                _logger.LogInformation("Processed task updated message for TaskId: {TaskId}", message.TaskId);
            }
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
