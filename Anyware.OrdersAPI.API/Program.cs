
using Anyware.OrdersAPI.Domain.Interfaces;
using Anyware.OrdersAPI.Infrastructure.Caching;
using Anyware.OrdersAPI.Infrastructure.Data;
using Anyware.OrdersAPI.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Anyware.OrdersAPI.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddInfrastructure();

            // Add Redis as a singleton
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var connectionString = builder.Configuration.GetConnectionString("Redis")
                    ?? throw new InvalidOperationException("Redis connection string not configured");

                var options = ConfigurationOptions.Parse(connectionString);
                options.AbortOnConnectFail = false;  

                return ConnectionMultiplexer.Connect(options);
            });

            // Register your custom Redis service
            builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

            // Database configuration
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy => policy.AllowAnyOrigin()
                                   .AllowAnyMethod()
                                   .AllowAnyHeader());
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseAuthorization();


            app.MapControllers();

            // Apply migrations automatically
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.Migrate();
            }

            app.Run();
        }
    }
}
