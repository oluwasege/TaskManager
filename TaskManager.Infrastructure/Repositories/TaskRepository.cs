using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManager.Infrastructure.Persistence;
using TaskStatus = TaskManager.Domain.Entities.TaskStatus;

namespace TaskManager.Infrastructure.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Domain.Entities.Task> GetByIdAsync(Guid id)
        {
            return await _context.Tasks.FindAsync(id);
        }

        public async Task<Domain.Entities.Task> GetByTitleAsync(string task)
        {
            return await _context.Tasks.FirstOrDefaultAsync(x=>x.Title.ToLower()==task.ToLower());
        }

        public async Task<(List<Domain.Entities.Task>, int)> GetAllAsync(int page, int pageSize)
        {
            var totalCount = await _context.Tasks.CountAsync();
            var tasks = await _context.Tasks
                .OrderByDescending(t => t.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (tasks, totalCount);
        }

        public async Task<(List<Domain.Entities.Task>, int)> GetByStatusAsync(TaskStatus status, int page, int pageSize)
        {
            var totalCount = await _context.Tasks.CountAsync(t => t.Status == status);
            var tasks = await _context.Tasks
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (tasks, totalCount);
        }

        public async Task AddAsync(Domain.Entities.Task task)
        {
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Domain.Entities.Task task)
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }
    }
}

