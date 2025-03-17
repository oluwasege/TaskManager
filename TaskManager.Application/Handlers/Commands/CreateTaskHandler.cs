using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Commands;
using TaskStatus = TaskManager.Domain.Entities.TaskStatus;

namespace TaskManager.Application.Handlers.Commands
{
    public class CreateTaskHandler
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IMessageProducer _messageProducer;
        private readonly ILogger<CreateTaskHandler> _logger;

        public CreateTaskHandler(ITaskRepository taskRepository, IMessageProducer messageProducer, ILogger<CreateTaskHandler> logger)
        {
            _taskRepository = taskRepository;
            _messageProducer = messageProducer;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateTaskCommand command)
        {
            _logger.LogInformation("Creating new task with title: {Title}", command.Title);

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
            _messageProducer.PublishTaskCreated(new TaskCreatedMessage { TaskId = task.Id });

            return task.Id;
        }
    }
