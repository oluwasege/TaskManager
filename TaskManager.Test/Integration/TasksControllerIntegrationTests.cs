using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.Commands;
using TaskManager.Application.Models;
using Xunit;
using TaskManager.API;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Messaging;
using TaskManager.Infrastructure.Caching;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TaskManager.Test.Integration
{
    public class TasksControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public TasksControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace real services with in-memory alternatives for testing
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("InMemoryDbForTesting");
                    });

                    // Mock Redis and RabbitMQ services
                    services.AddSingleton<ICacheService>(new Mock<ICacheService>().Object);
                    services.AddSingleton<IMessageProducer>(new Mock<IMessageProducer>().Object);

                    // Remove RabbitMQ consumer for tests
                    var consumerDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IHostedService) &&
                             d.ImplementationType == typeof(RabbitMqConsumer));

                    if (consumerDescriptor != null)
                    {
                        services.Remove(consumerDescriptor);
                    }
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task CreateTask_ReturnsCreatedStatusCode()
        {
            // Arrange
            var task = new CreateTaskCommand
            {
                Title = "Integration Test Task",
                Description = "Test Description",
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/tasks", task);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var taskId = await response.Content.ReadFromJsonAsync<Guid>();
            Assert.NotEqual(Guid.Empty, taskId);
        }

        [Fact]
        public async Task GetAllTasks_ReturnsOkStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/api/tasks");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<TaskDto>>();
            Assert.NotNull(result);
        }
    }
}
