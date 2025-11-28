
using Anyware.OrdersAPI.API.Middleware;
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

            // Configure logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Anyware Orders API",
                    Version = "v1",
                    Description = "A professional Orders API with Redis caching and Clean Architecture 'Anyware Software'"
                });
            });

            // Register Infrastructure services
            builder.Services.AddInfrastructure();

            // Add Redis as a singleton
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<Program>>();
                var connectionString = builder.Configuration.GetConnectionString("Redis")
                    ?? throw new InvalidOperationException("Redis connection string not configured");

                var options = ConfigurationOptions.Parse(connectionString);
                options.AbortOnConnectFail = false;
                options.ConnectRetry = 3;
                options.ConnectTimeout = 5000;
                options.SyncTimeout = 5000;

                var multiplexer = ConnectionMultiplexer.Connect(options);

                multiplexer.ConnectionFailed += (sender, args) =>
                {
                    logger.LogError("Redis connection failed: {EndPoint} - {FailureType}", args.EndPoint, args.FailureType);
                };

                multiplexer.ConnectionRestored += (sender, args) =>
                {
                    logger.LogInformation("Redis connection restored: {EndPoint}", args.EndPoint);
                };

                return multiplexer;
            });



            // Register your custom Redis service
            builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

            // Database configuration
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null);
                    }));

            // CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy => policy.AllowAnyOrigin()
                                   .AllowAnyMethod()
                                   .AllowAnyHeader());
            });

            var app = builder.Build();

            // Global exception handling middleware
            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

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
