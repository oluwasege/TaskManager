using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Commands;
using TaskManager.Infrastructure.Messaging;
using TaskManager.Infrastructure.Repositories;
using TaskStatus = TaskManager.Domain.Entities.TaskStatus;

namespace TaskManager.Application.Handlers.Commands
{
    public class CreateTaskHandler : ICreateTaskHandler
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ILogger<CreateTaskHandler> _logger;
        private readonly IMessageProducer _messageProducer;

        public CreateTaskHandler(
            ITaskRepository taskRepository,
            ILogger<CreateTaskHandler> logger,
            IMessageProducer messageProducer)
        {
            _taskRepository = taskRepository;
            _logger = logger;
            _messageProducer = messageProducer;
        }

        public async Task<Guid> Handle(CreateTaskCommand command)
        {
            _logger.LogInformation("Creating new task with title: {Title}", command.Title);
            var existingTask = await _taskRepository.GetByTitleAsync(command.Title);
            if (existingTask is null)
            {
                _logger.LogWarning("Task with title '{Title}' already exists", command.Title);
                throw new InvalidOperationException($"Task with title '{command.Title}' already exists");
            }
            var task = new Domain.Entities.Task
            {
                Id = Guid.NewGuid(),
                Title = command.Title,
                Description = command.Description,
                Status = TaskStatus.Pending,
                DueDate = command.DueDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _taskRepository.AddAsync(task);

            // Publish message for asynchronous processing
            await _messageProducer.PublishTaskCreated(new TaskCreatedMessage { TaskId = task.Id, IsNew = true });
            return task.Id;
        }
    }
}
