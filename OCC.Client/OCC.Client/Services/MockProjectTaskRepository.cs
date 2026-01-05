using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OCC.Shared.Models;

namespace OCC.Client.Services
{
    public class MockProjectTaskRepository : IRepository<ProjectTask>
    {
        private readonly List<ProjectTask> _tasks;

        public MockProjectTaskRepository()
        {
            _tasks = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Site Preparation",
                    Description = "Clear site and setup perimeter fencing",
                    StartDate = DateTime.Now.AddDays(-5),
                    FinishDate = DateTime.Now.AddDays(2),
                    Status = "In Progress",
                    ProjectId = Guid.Parse("11111111-2222-3333-4444-555555555555"),
                    Priority = "High",
                    AssignedTo = "John Smith"
                },
                new ProjectTask
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Foundation Pouring",
                    Description = "Pour concrete foundation",
                    StartDate = DateTime.Now.AddDays(3),
                    FinishDate = DateTime.Now.AddDays(5),
                    Status = "To Do",
                    ProjectId = Guid.Parse("11111111-2222-3333-4444-555555555555"),
                    Priority = "Medium",
                    AssignedTo = "Jane Doe"
                }
            };
        }

        public async Task<IEnumerable<ProjectTask>> GetAllAsync()
        {
            return await Task.FromResult(_tasks);
        }

        public async Task<ProjectTask?> GetByIdAsync(Guid id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            return await Task.FromResult(task);
        }

        public async Task AddAsync(ProjectTask entity)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }
            _tasks.Add(entity);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(ProjectTask entity)
        {
            var existing = _tasks.FirstOrDefault(t => t.Id == entity.Id);
            if (existing != null)
            {
                _tasks.Remove(existing);
                _tasks.Add(entity);
            }
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var existing = _tasks.FirstOrDefault(t => t.Id == id);
            if (existing != null)
            {
                _tasks.Remove(existing);
            }
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<ProjectTask>> FindAsync(System.Linq.Expressions.Expression<Func<ProjectTask, bool>> predicate)
        {
            var compiled = predicate.Compile();
            return await Task.FromResult(_tasks.Where(compiled).ToList());
        }
    }
}
