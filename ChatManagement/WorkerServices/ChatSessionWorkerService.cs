using ChatManagement.IServices;
using ChatManagement.Models;
using ChatManagement.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ChatManagement.WorkerServices
{
    public class ChatSessionWorkerService : BackgroundService
    {
        private readonly ILogger<ChatSessionWorkerService> _logger;
        private readonly RabbitMQService _rabbitMQService;
        private readonly IChatManagementService _chatManagementService;

        public ChatSessionWorkerService(
            ILogger<ChatSessionWorkerService> logger, 
            RabbitMQService rabbitMQService,
            IChatManagementService chatManagementService)
        {
            _logger = logger;
            _rabbitMQService = rabbitMQService;
            _chatManagementService = chatManagementService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_rabbitMQService.Channel);
            consumer.Received += (ch, ea) =>
            {
                // received message
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());

                // handle the received message
                HandleMessage(content);
                _rabbitMQService.Channel.BasicAck(ea.DeliveryTag, false);
            };

            _rabbitMQService.Channel.BasicConsume("ChatSessionsQueue", false, consumer);
            return Task.CompletedTask;
        }

        private void HandleMessage(string content)
        {
            var chatSession = JsonConvert.DeserializeObject<ChatSession>(content);

            var agent = _chatManagementService.GetNextAvailableAgent();

            if(agent != null)
            {
                _chatManagementService.AssignChat(agent, chatSession);
                
                _logger.LogInformation($"Chat session received: {content}");

                
                agent.IsAvailable = true;
                _chatManagementService.DecrementQueueLength();
                Console.WriteLine($" {chatSession.SessionId} - consumed by {agent.Id}");
            }
            else
            {
                _logger.LogWarning($"No available agent for chat session {chatSession.SessionId}");
            }
        }
    }
}
