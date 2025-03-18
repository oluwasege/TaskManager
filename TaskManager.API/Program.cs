
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;
using System.Reflection;
using TaskManager.Application.Handlers.Commands;
using TaskManager.Application.Handlers.Queries;
using TaskManager.Domain.StateMachine;
using TaskManager.Infrastructure.Caching;
using TaskManager.Infrastructure.Messaging;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Task Management API",
                    Version = "v1",
                    Description = "A microservices-based Task Management API built with .NET Core",
                    Contact = new OpenApiContact
                    {
                        Name = "Tingtel Tech",
                        Email = "info@tingteltech.com",
                        Url = new Uri("https://tingteltech.com")
                    }
                });

                // Include XML comments in Swagger documentation
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                //c.IncludeXmlComments(xmlPath);
            });

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/task-manager-.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog();

            // Add database context
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            // Add Redis cache
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configuration = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis"));
                return ConnectionMultiplexer.Connect(configuration);
            });

            // Register application services
            builder.Services.AddScoped<ITaskRepository, TaskRepository>();
            builder.Services.AddSingleton<ICacheService, RedisCacheService>();
            //builder.Services.AddSingleton<RabbitMqProducer>(sp =>
            //{
            //    var logger = sp.GetRequiredService<ILogger<RabbitMqProducer>>();
            //    var config = sp.GetRequiredService<IConfiguration>();
            //    // Block on the async initialization.
            //    return RabbitMqProducer.CreateAsync(config, logger).GetAwaiter().GetResult();
            //});
            builder.Services.AddSingleton<IMessageProducer, RabbitMqProducer>();
            builder.Services.AddHostedService<RabbitMqConsumer>();
            builder.Services.AddSingleton<TaskStateMachine>();

            // Register command and query handlers
            builder.Services.AddScoped<ICreateTaskHandler, CreateTaskHandler>();
            builder.Services.AddScoped<IUpdateTaskHandler,UpdateTaskHandler>();
            builder.Services.AddScoped<UpdateTaskStatusHandler>();
            builder.Services.AddScoped<IDeleteTaskHandler,DeleteTaskHandler>();
            builder.Services.AddScoped<GetTaskByIdHandler>();
            builder.Services.AddScoped<GetAllTasksHandler>();
            builder.Services.AddScoped<GetTasksByStatusHandler>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseSerilogRequestLogging();

            app.MapControllers();

            // Apply database migrations on startup
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();
            }


            app.Run();
        }
    }
}
