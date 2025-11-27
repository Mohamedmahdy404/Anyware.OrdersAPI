using Anyware.OrdersAPI.Domain.Entities;
using Anyware.OrdersAPI.Domain.Interfaces;
using Anyware.OrdersAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anyware.OrdersAPI.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }
         
        public async Task<Order?> GetByIdAsync(Guid id)
        {
            return await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByCustomerNameAsync(string customerName)
        {
            return await _context.Orders
                .Where(o => o.CustomerName.Contains(customerName))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
        }

        public Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
                _context.Orders.Remove(order);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }

}

