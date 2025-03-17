using TaskStatus = TaskManager.Domain.Entities.TaskStatus;
namespace TaskManager.Infrastructure.Repositories
{
    public interface ITaskRepository
    {
        Task<Task> GetByIdAsync(Guid id);
        Task<(List<Task>, int)> GetAllAsync(int page, int pageSize);
        Task<(List<Task>, int)> GetByStatusAsync(TaskStatus status, int page, int pageSize);
        Task AddAsync(Task task);
        Task UpdateAsync(Task task);
        Task DeleteAsync(Guid id);
    }
}
