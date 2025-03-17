using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Commands;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Handlers.Commands;
using TaskManager.Application.Handlers.Queries;
using TaskManager.Application.Models;
using TaskManager.Application.Queries;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TasksController : ControllerBase
    {
        private readonly ICreateTaskHandler _createTaskHandler;
        private readonly UpdateTaskHandler _updateTaskHandler;
        private readonly UpdateTaskStatusHandler _updateTaskStatusHandler;
        private readonly DeleteTaskHandler _deleteTaskHandler;
        private readonly GetTaskByIdHandler _getTaskByIdHandler;
        private readonly GetAllTasksHandler _getAllTasksHandler;
        private readonly GetTasksByStatusHandler _getTasksByStatusHandler;
        private readonly ILogger<TasksController> _logger;

        public TasksController(
        ICreateTaskHandler createTaskHandler,
        UpdateTaskHandler updateTaskHandler,
        UpdateTaskStatusHandler updateTaskStatusHandler,
        DeleteTaskHandler deleteTaskHandler,
        GetTaskByIdHandler getTaskByIdHandler,
        GetAllTasksHandler getAllTasksHandler,
        GetTasksByStatusHandler getTasksByStatusHandler,
        ILogger<TasksController> logger)
        {
            _createTaskHandler = createTaskHandler;
            _updateTaskHandler = updateTaskHandler;
            _updateTaskStatusHandler = updateTaskStatusHandler;
            _deleteTaskHandler = deleteTaskHandler;
            _getTaskByIdHandler = getTaskByIdHandler;
            _getAllTasksHandler = getAllTasksHandler;
            _getTasksByStatusHandler = getTasksByStatusHandler;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new task
        /// </summary>
        /// <param name="command">Task creation data</param>
        /// <returns>The ID of the created task</returns>
        /// <response code="201">Returns the ID of the created task</response>
        /// <response code="400">If the command is invalid</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> CreateTask(CreateTaskCommand command)
        {
            _logger.LogInformation("Request received to create a new task");

            var taskId = await _createTaskHandler.Handle(command);

            return CreatedAtAction(nameof(GetTaskById), new { id = taskId }, taskId);
        }

        /// <summary>
        /// Updates an existing task
        /// </summary>
        /// <param name="id">Task ID</param>
        /// <param name="command">Task update data</param>
        /// <returns>No content</returns>
        /// <response code="204">Task successfully updated</response>
        /// <response code="404">Task not found</response>
        /// <response code="400">If the command is invalid</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateTask(Guid id, UpdateTaskCommand command)
        {
            _logger.LogInformation("Request received to update task with ID: {Id}", id);

            command.Id = id;

            try
            {
                await _updateTaskHandler.Handle(command);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Error");
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Updates the status of a task
        /// </summary>
        /// <param name="id">Task ID</param>
        /// <param name="command">Status update data</param>
        /// <returns>No content</returns>
        /// <response code="204">Task status successfully updated</response>
        /// <response code="404">Task not found</response>
        /// <response code="400">If the status transition is invalid</response>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateTaskStatus(Guid id, UpdateTaskStatusCommand command)
        {
            _logger.LogInformation("Request received to update status of task with ID: {Id}", id);

            command.Id = id;

            try
            {
                await _updateTaskStatusHandler.Handle(command);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Error");
                return NotFound(ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.LogWarning(ex, "Error");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a task
        /// </summary>
        /// <param name="id">Task ID</param>
        /// <returns>No content</returns>
        /// <response code="204">Task successfully deleted</response>
        /// <response code="404">Task not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            _logger.LogInformation("Request received to delete task with ID: {Id}", id);

            try
            {
                await _deleteTaskHandler.Handle(new DeleteTaskCommand { Id = id });
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Error");
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Gets a task by its ID
        /// </summary>
        /// <param name="id">Task ID</param>
        /// <returns>The task</returns>
        /// <response code="200">Returns the task</response>
        /// <response code="404">Task not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaskDto>> GetTaskById(Guid id)
        {
            _logger.LogInformation("Request received to get task with ID: {Id}", id);

            try
            {
                var task = await _getTaskByIdHandler.Handle(new GetTaskByIdQuery { Id = id });
                return Ok(task);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Gets all tasks with pagination
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>List of tasks</returns>
        /// <response code="200">Returns the list of tasks</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<TaskDto>>> GetAllTasks([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Request received to get all tasks (page: {Page}, pageSize: {PageSize})", page, pageSize);

            var tasks = await _getAllTasksHandler.Handle(new GetAllTasksQuery { Page = page, PageSize = pageSize });
            return Ok(tasks);
        }

        /// <summary>
        /// Gets tasks by status with pagination
        /// </summary>
        /// <param name="status">Task status</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>List of tasks with the specified status</returns>
        /// <response code="200">Returns the list of tasks</response>
        /// <response code="400">If the status is invalid</response>
        [HttpGet("status/{status}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedResult<TaskDto>>> GetTasksByStatus(
            Domain.Entities.TaskStatus status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Request received to get tasks with status {Status} (page: {Page}, pageSize: {PageSize})",
                status, page, pageSize);

            var tasks = await _getTasksByStatusHandler.Handle(
                new GetTasksByStatusQuery { Status = status, Page = page, PageSize = pageSize });

            return Ok(tasks);
        }
    }
}
