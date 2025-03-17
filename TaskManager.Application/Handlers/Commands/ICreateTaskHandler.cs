using TaskManager.Application.Commands;

namespace TaskManager.Application.Handlers.Commands
{
    public interface ICreateTaskHandler
    {
        Task<Guid> Handle(CreateTaskCommand command);
    }
}