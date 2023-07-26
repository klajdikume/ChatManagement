using ChatManagement.Models;
using ChatManagement.Services;
using ChatManagement.WorkerServices;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<RabbitMQService>();
builder.Services.AddSingleton<ChatManagementService>();
builder.Services.AddSingleton<AgentChatCoordinatorService>();
builder.Services.AddHostedService<ChatSessionWorkerService>();
builder.Services.AddHostedService<SessionMonitorService>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Get the instance of AgentChatCoordinatorService from the DI container
var agentChatCoordinatorService = app.Services.GetService<AgentChatCoordinatorService>();

// Start listening
agentChatCoordinatorService?.StartListening();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    var chatManagementService = app.Services.GetService<ChatManagementService>();
    if (chatManagementService != null)
    {
        chatManagementService.AddTeam(new Team { Id = 1, Name = "Team A"
            , Agents = new List<Agent>() { new Agent { Id = 1, Seniority=AgentSeniority.TeamLead, IsAvailable = true }, new Agent { Id = 2, Seniority = AgentSeniority.MidLevel, IsAvailable = true }, new Agent { Id = 2, Seniority = AgentSeniority.Junior, IsAvailable = true } }
            , ShiftStart = DateTime.Now, ShiftEnd = DateTime.Now.AddHours(8) });
        chatManagementService.AddTeam(new Team { Id = 2, Name = "Team B", Agents = new List<Agent>() { new Agent { Id = 1, Seniority = AgentSeniority.Senior, IsAvailable = true }, new Agent { Id = 2, Seniority = AgentSeniority.MidLevel, IsAvailable = true }, new Agent { Id = 3, Seniority = AgentSeniority.Junior, IsAvailable = true }, new Agent { Id = 4, Seniority = AgentSeniority.Junior, IsAvailable = true } }, ShiftStart = DateTime.Now.AddHours(8), ShiftEnd = DateTime.Now.AddHours(16) });
        chatManagementService.AddTeam(new Team { Id = 3, Name = "Team C", Agents = new List<Agent>() { new Agent { Id = 1, Seniority = AgentSeniority.MidLevel, IsAvailable = true }, new Agent { Id = 2, Seniority = AgentSeniority.MidLevel, IsAvailable = true } }, ShiftStart = DateTime.Now.AddHours(16), ShiftEnd = DateTime.Now.AddHours(24) });
        chatManagementService.AddTeam(new Team { Id = 4, Name = "overflow", Agents = new List<Agent>() { new Agent { Id = 1, Seniority = AgentSeniority.Junior, IsAvailable = true }, new Agent { Id = 2, Seniority = AgentSeniority.Junior, IsAvailable = true }, new Agent { Id = 3, Seniority = AgentSeniority.Junior, IsAvailable = true } },  });
    }
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Services.GetService<RabbitMQService>()?.CreateQueue("ChatSessionsQueue");

app.Run();
