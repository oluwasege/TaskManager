using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.Commands;

namespace TaskManager.Application.Handlers.Commands
{
    public interface IDeleteTaskHandler
    {
        Task Handle(DeleteTaskCommand command);
    }
}
