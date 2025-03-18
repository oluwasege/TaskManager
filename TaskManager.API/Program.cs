
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;
using System.Reflection;
using TaskManager.API.Configurations;
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
            builder.Services.AddConfiguration(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.ConfigureApp(builder.Configuration);
            app.MapControllers();
            app.RunAsync();
        }
    }
}
