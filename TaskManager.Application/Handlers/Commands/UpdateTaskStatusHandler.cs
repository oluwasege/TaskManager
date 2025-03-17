using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Commands;
using TaskManager.Application.Exceptions;
using TaskManager.Domain.StateMachine;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.Application.Handlers.Commands
{
    public class UpdateTaskStatusHandler
    {
        private readonly ITaskRepository _taskRepository;
        private readonly TaskStateMachine _stateMachine;
        private readonly ILogger<UpdateTaskStatusHandler> _logger;

        public UpdateTaskStatusHandler(
            ITaskRepository taskRepository,
            TaskStateMachine stateMachine,
            ILogger<UpdateTaskStatusHandler> logger)
        {
            _taskRepository = taskRepository;
            _stateMachine = stateMachine;
            _logger = logger;
        }

        public async Task Handle(UpdateTaskStatusCommand command)
        {
            _logger.LogInformation("Updating task status for ID: {Id} to {Status}", command.Id, command.NewStatus);
            var task = await _taskRepository.GetByIdAsync(command.Id);
            if (task == null)
            {
                _logger.LogWarning("Task with ID {Id} not found", command.Id);
                throw new NotFoundException($"Task with ID {command.Id} not found");
            }

            try
            {
                task.Status = _stateMachine.Transition(task.Status, command.NewStatus);
                task.UpdatedAt = DateTime.UtcNow;
                await _taskRepository.UpdateAsync(task);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid status transition for task {Id}", command.Id);
                throw new BadRequestException(ex.Message);
            }
        }
    }
}
