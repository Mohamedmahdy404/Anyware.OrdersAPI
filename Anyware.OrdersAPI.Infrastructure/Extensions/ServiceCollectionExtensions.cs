using Anyware.OrdersAPI.Application.Interfaces;
using Anyware.OrdersAPI.Application.Services;
using Anyware.OrdersAPI.Domain.Interfaces;
using Anyware.OrdersAPI.Infrastructure.Caching;
using Anyware.OrdersAPI.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anyware.OrdersAPI.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IRedisCacheService, RedisCacheService>();


            return services;
        }
    }
}
