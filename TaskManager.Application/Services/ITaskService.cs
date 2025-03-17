using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.Commands;

namespace TaskManager.Application.Services
{
    public interface ITaskService
    {
        Task<string> CreateTaskAsync(CreateTaskCommand model);
    }
}
