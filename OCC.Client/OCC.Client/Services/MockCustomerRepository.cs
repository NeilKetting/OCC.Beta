using OCC.Client.Services;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OCC.Client.Services
{
    public class MockCustomerRepository : IRepository<Customer>
    {
        private readonly List<Customer> _customers;

        public MockCustomerRepository()
        {
            _customers = new List<Customer>
            {
                new Customer { Name = "Internal", Header="Internal", Email="projects@internal.com" },
                new Customer { Name = "Acme Corp", Header="External", Email="contact@acme.com" },
                new Customer { Name = "Globex", Header="External", Email="info@globex.null" },
                new Customer { Name = "Soylent Corp", Header="External", Email="people@soylent.green" }
            };
        }

        public Task<IEnumerable<Customer>> GetAllAsync() => Task.FromResult(_customers.AsEnumerable());

        public Task<Customer?> GetByIdAsync(Guid id) => Task.FromResult(_customers.FirstOrDefault(c => c.Id == id));

        public Task<IEnumerable<Customer>> FindAsync(Expression<Func<Customer, bool>> predicate)
        {
            return Task.FromResult(_customers.AsQueryable().Where(predicate).AsEnumerable());
        }

        public Task AddAsync(Customer entity)
        {
            _customers.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Customer entity)
        {
            var existing = _customers.FirstOrDefault(c => c.Id == entity.Id);
            if (existing != null)
            {
                _customers.Remove(existing);
                _customers.Add(entity);
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Customer id)
        {
             // Interface mismatch? 
             // IRepository<T> usually has DeleteAsync(Guid id).
             // Let's check IRepository definition again.
             // It is DeleteAsync(Guid id).
             return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            var existing = _customers.FirstOrDefault(c => c.Id == id);
            if (existing != null)
            {
                _customers.Remove(existing);
            }
            return Task.CompletedTask;
        }
    }
}
