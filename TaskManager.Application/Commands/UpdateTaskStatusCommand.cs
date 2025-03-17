using TaskStatus = TaskManager.Domain.Entities.TaskStatus;
namespace TaskManager.Application.Commands
{
    public class UpdateTaskStatusCommand
    {
        public Guid Id { get; set; }
        public TaskStatus NewStatus { get; set; }
    }
}
