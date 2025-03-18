using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Commands;
using TaskManager.Application.Exceptions;
using TaskManager.Infrastructure.Messaging;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.Application.Handlers.Commands
{
    public class UpdateTaskHandler : IUpdateTaskHandler
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ILogger<UpdateTaskHandler> _logger;
        //private readonly IMessageProducer _messageProducer;

        public UpdateTaskHandler(
            ITaskRepository taskRepository, 
            ILogger<UpdateTaskHandler> logger)
        {
            _taskRepository = taskRepository;
            _logger = logger;
            //_messageProducer = messageProducer;
        }

        public async Task Handle(UpdateTaskCommand command)
        {
            _logger.LogInformation("Updating task with ID: {Id}", command.Id);

            var task = await _taskRepository.GetByIdAsync(command.Id);
            if (task == null)
            {
                _logger.LogWarning("Task with ID {Id} not found", command.Id);
                throw new NotFoundException($"Task with ID {command.Id} not found");
            }

            task.Title = command.Title;
            task.Description = command.Description;
            task.DueDate = command.DueDate;
            task.UpdatedAt = DateTime.UtcNow;

            await _taskRepository.UpdateAsync(task);

            // Publish message for asynchronous processing
           // await _messageProducer.PublishTaskCreated(new TaskCreatedMessage { TaskId = task.Id, IsNew = false });
        }

    }
}