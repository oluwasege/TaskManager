using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Models;
using TaskManager.Application.Queries;

namespace TaskManager.Application.Handlers.Queries
{
    public class GetTaskByIdHandler
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetTaskByIdHandler> _logger;

        public GetTaskByIdHandler(
            ITaskRepository taskRepository,
            ICacheService cacheService,
            ILogger<GetTaskByIdHandler> logger)
        {
            _taskRepository = taskRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<TaskDto> Handle(GetTaskByIdQuery query)
        {
            _logger.LogInformation("Getting task with ID: {Id}", query.Id);

            // Try to get from cache first
            var cacheKey = $"task:{query.Id}";
            var cachedTask = await _cacheService.GetAsync<TaskDto>(cacheKey);
            if (cachedTask != null)
            {
                _logger.LogInformation("Task with ID {Id} retrieved from cache", query.Id);
                return cachedTask;
            }

            // If not in cache, get from database
            var task = await _taskRepository.GetByIdAsync(query.Id);
            if (task == null)
            {
                _logger.LogWarning("Task with ID {Id} not found", query.Id);
                throw new NotFoundException($"Task with ID {query.Id} not found");
            }

            var taskDto = new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };

            // Store in cache
            await _cacheService.SetAsync(cacheKey, taskDto, TimeSpan.FromMinutes(5));

            return taskDto;
        }
    }
}
