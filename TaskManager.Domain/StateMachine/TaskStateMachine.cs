global using TaskStatus = TaskManager.Domain.Entities.TaskStatus;

namespace TaskManager.Domain.StateMachine
{
    public class TaskStateMachine
    {
        private static readonly Dictionary<TaskStatus, TaskStatus[]> _allowedTransitions = new()
        {
            { TaskStatus.Pending, new[] { TaskStatus.InProgress, TaskStatus.Canceled } },
            { TaskStatus.InProgress, new[] { TaskStatus.Completed, TaskStatus.Blocked, TaskStatus.Canceled } },
            { TaskStatus.Blocked, new[] { TaskStatus.InProgress, TaskStatus.Canceled } },
            { TaskStatus.Completed, new[] { TaskStatus.InProgress } },
            { TaskStatus.Canceled, new[] { TaskStatus.Pending } }
        };

        public static bool CanTransition(TaskStatus currentStatus, TaskStatus newStatus)
        {
            if (!_allowedTransitions.TryGetValue(currentStatus, out var allowedTransitions))
                return false;

            return allowedTransitions.Contains(newStatus);
        }

        public static TaskStatus Transition(TaskStatus currentStatus, TaskStatus newStatus)
        {
            if (!CanTransition(currentStatus, newStatus))
                throw new InvalidOperationException($"Cannot transition from {currentStatus} to {newStatus}");

            return newStatus;
        }
    }
}
