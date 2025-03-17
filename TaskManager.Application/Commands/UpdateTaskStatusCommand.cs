using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Application.Commands
{
    public class UpdateTaskStatusCommand
    {
        public Guid Id { get; set; }
        public TaskStatus NewStatus { get; set; }
    }
}
