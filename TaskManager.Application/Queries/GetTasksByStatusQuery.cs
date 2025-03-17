using TaskStatus = TaskManager.Domain.Entities.TaskStatus;

namespace TaskManager.Application.Queries
{
    public class GetTasksByStatusQuery
    {
        public TaskStatus Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
