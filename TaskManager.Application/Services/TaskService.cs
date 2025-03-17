using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.Commands;
using TaskManager.Infrastructure.Messaging;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly RabbitMqProducer _messageProducer;
        private readonly ILogger<TaskService> _logger;

        public TaskService(
            ITaskRepository taskRepository, 
            RabbitMqProducer messageProducer, 
            ILogger<TaskService> logger)
        {
            _taskRepository = taskRepository;
            _messageProducer = messageProducer;
            _logger = logger;
        }

        public async Task<string> CreateTaskAsync(CreateTaskCommand model)
        {
            _logger.LogInformation("Creating new task with title: {Title}", model.Title);
            
            // Publish message for asynchronous processing
            await _messageProducer.PublishMessage<CreateTaskCommand>(model);
            return "Task creating";
        }
    }
}
