using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.Commands;
using TaskManager.Application.Handlers.Commands;
using TaskManager.Infrastructure.Messaging;
using TaskManager.Infrastructure.Repositories;
using Xunit;
using TaskStatus = TaskManager.Domain.Entities.TaskStatus;

namespace TaskManager.Tests.Unit
{
    public class CreateTaskHandlerTests
    {
        private readonly Mock<ITaskRepository> _mockRepository;
        private readonly Mock<IMessageProducer> _mockProducer;
        private readonly Mock<ILogger<CreateTaskHandler>> _mockLogger;
        private readonly CreateTaskHandler _handler;

        public CreateTaskHandlerTests()
        {
            _mockRepository = new Mock<ITaskRepository>();
            _mockProducer = new Mock<IMessageProducer>();
            _mockLogger = new Mock<ILogger<CreateTaskHandler>>();
            _handler = new CreateTaskHandler(_mockRepository.Object, _mockLogger.Object, _mockProducer.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_AddsTaskAndPublishesMessage()
        {
            // Arrange
            var command = new CreateTaskCommand
            {
                Title = "Test Task",
                Description = "Test Description",
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Task>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _mockRepository.Verify(r => r.AddAsync(It.Is<Domain.Entities.Task>(t =>
                t.Title == command.Title &&
                t.Description == command.Description)),
                Times.Once);
            _mockProducer.Verify(p => p.PublishTaskCreated(It.Is<TaskCreatedMessage>(m =>
                m.TaskId == result)),
                Times.Once);
        }
    }
}
