using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;
using StackExchange.Redis;
using System.Reflection;
using System.Text.Json.Serialization;
using TaskManager.Application.Handlers.Commands;
using TaskManager.Application.Handlers.Queries;
using TaskManager.Domain.StateMachine;
using TaskManager.Infrastructure.Caching;
using TaskManager.Infrastructure.Helpers;
using TaskManager.Infrastructure.Messaging;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Repositories;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;

namespace TaskManager.API.Configurations
{
    public static class ServiceExtension
    {
        public static void AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            //set the database to use SQL Server.
            services.AddDbContext<ApplicationDbContext>(
              options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
              p =>
              {
                  p.EnableRetryOnFailure();
                  p.MaxBatchSize(150);
              }));


            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var redisConfiguration = ConfigurationOptions.Parse(configuration.GetConnectionString("Redis"));
                return ConnectionMultiplexer.Connect(redisConfiguration);
            });

            var rabbitMqsettings = configuration.GetSection(nameof(RabbitMQConfig));
            services.Configure<RabbitMQConfig>(rabbitMqsettings);

            Log.Logger = new LoggerConfiguration()
               .WriteTo.Console()
               .CreateBootstrapLogger();

            Log.Information("Starting up!");

            services.AddSerilog((services, lc) => lc
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Async(a => a.File("logs/log.txt", rollingInterval: RollingInterval.Day))
            .WriteTo.Async(a => a.Console(new ExpressionTemplate(
            // Include trace and span ids when present.
            "[{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m}\n{@x}",
            theme: TemplateTheme.Code)))
            );

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Task Management API",
                    Version = "v1",
                    Description = "A Task Management API built with .NET Core",
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
                c.AddEnumsWithValuesFixFilters();
                c.IncludeXmlComments(xmlPath);
            });

            services.AddEndpointsApiExplorer();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();


            // Register application services
            services.AddScoped<ITaskRepository, TaskRepository>();
            services.AddSingleton<ICacheService, RedisCacheService>();

            services.AddSingleton<IMessageProducer, RabbitMqProducer>();
            services.AddHostedService<RabbitMqConsumer>();
            services.AddSingleton<TaskStateMachine>();

            // Register command and query handlers
            services.AddScoped<ICreateTaskHandler, CreateTaskHandler>();
            services.AddScoped<IUpdateTaskHandler, UpdateTaskHandler>();
            services.AddScoped<UpdateTaskStatusHandler>();
            services.AddScoped<IDeleteTaskHandler, DeleteTaskHandler>();
            services.AddScoped<GetTaskByIdHandler>();
            services.AddScoped<GetAllTasksHandler>();
            services.AddScoped<GetTasksByStatusHandler>();
        }

        public static void ConfigureApp(this IApplicationBuilder app, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(app);
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseXContentTypeOptions();
            app.UseXfo(xfo => xfo.Deny());
            app.UseXXssProtection(options => options.EnabledWithBlockMode());
            app.UseCsp(options => options
                .BlockAllMixedContent()
                .StyleSources(s => s.Self())
                .FontSources(s => s.Self())
                .FormActions(s => s.Self())
                .FrameAncestors(s => s.Self())
                .ImageSources(s => s.Self())
                .ScriptSources(s => s.Self()));

            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseExceptionHandler();
            app.UseSerilogRequestLogging();
        }
    }
}
