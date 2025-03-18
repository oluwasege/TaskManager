using TaskManager.Application.Commands;

namespace TaskManager.Application.Handlers.Commands
{
    public interface IUpdateTaskHandler
    {
        Task Handle(UpdateTaskCommand command);
    }
}