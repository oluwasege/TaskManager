using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Commands;
using TaskManager.Application.Exceptions;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.Application.Handlers.Commands
{
    public class DeleteTaskHandler
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ILogger<DeleteTaskHandler> _logger;

        public DeleteTaskHandler(ITaskRepository taskRepository, ILogger<DeleteTaskHandler> logger)
        {
            _taskRepository = taskRepository;
            _logger = logger;
        }

        public async Task Handle(DeleteTaskCommand command)
        {
            _logger.LogInformation("Deleting task with ID: {Id}", command.Id);

            var task = await _taskRepository.GetByIdAsync(command.Id);
            if (task == null)
            {
                _logger.LogWarning("Task with ID {Id} not found", command.Id);
                throw new NotFoundException($"Task with ID {command.Id} not found");
            }

            await _taskRepository.DeleteAsync(command.Id);
        }
    }
}

