using TaskStatus = TaskManager.Domain.Entities.TaskStatus;

namespace TaskManager.Infrastructure.Repositories
{
    public interface ITaskRepository
    {
        Task<Domain.Entities.Task> GetByIdAsync(Guid id);
        Task<(List<Domain.Entities.Task>, int)> GetAllAsync(int page, int pageSize);
        Task<(List<Domain.Entities.Task>, int)> GetByStatusAsync(TaskStatus status, int page, int pageSize);
        Task AddAsync(Domain.Entities.Task task);
        Task UpdateAsync(Domain.Entities.Task task);
        Task DeleteAsync(Guid id);
    }
}
