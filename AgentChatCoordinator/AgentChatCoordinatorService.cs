using ChatManagement.Models;
using ChatManagement.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace AgentChatCoordinator
{
    public class AgentChatCoordinatorService
    {
        private readonly IModel _channel;
        private readonly ChatManagementService _chatManagementService;

        public AgentChatCoordinatorService(IModel channel, ChatManagementService chatManagementService)
        {
            _channel = channel;
            _chatManagementService = chatManagementService;
        }

        public void StartListening()
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var chatSession = JsonConvert.DeserializeObject<ChatSession>(message);
                
                _chatManagementService.IncrementQueueLength();

                // Assign chat session to agent
                await _chatManagementService.AssignChatSession(chatSession);
            };

            _channel.BasicConsume(queue: "ChatSessionsQueue", autoAck: true, consumer: consumer);
        }
    }
}