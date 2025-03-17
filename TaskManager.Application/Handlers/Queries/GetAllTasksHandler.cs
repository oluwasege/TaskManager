﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Models;
using TaskManager.Application.Queries;
using TaskManager.Infrastructure.Caching;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.Application.Handlers.Queries
{
    public class GetAllTasksHandler
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetAllTasksHandler> _logger;

        public GetAllTasksHandler(
            ITaskRepository taskRepository,
            ICacheService cacheService,
            ILogger<GetAllTasksHandler> logger)
        {
            _taskRepository = taskRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<PagedResult<TaskDto>> Handle(GetAllTasksQuery query)
        {
            _logger.LogInformation("Getting all tasks page {Page} with page size {PageSize}", query.Page, query.PageSize);

            // Try to get from cache first
            var cacheKey = $"tasks:all:{query.Page}:{query.PageSize}";
            var cachedResult = await _cacheService.GetAsync<PagedResult<TaskDto>>(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogInformation("Tasks retrieved from cache");
                return cachedResult;
            }

            // If not in cache, get from database
            var (tasks, totalCount) = await _taskRepository.GetAllAsync(query.Page, query.PageSize);

            var taskDtos = tasks.Select(task => new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            }).ToList();

            var result = new PagedResult<TaskDto>
            {
                Items = taskDtos,
                TotalItems = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };

            // Store in cache
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(1));

            return result;
        }
    }
}
