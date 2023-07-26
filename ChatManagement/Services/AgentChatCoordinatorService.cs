using ChatManagement.Models;
using ChatManagement.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace ChatManagement.Services
{
    public class AgentChatCoordinatorService
    {
        private readonly RabbitMQService _rabbitMQService;
        private readonly ChatManagementService _chatManagementService;
        private IModel _channel;
        public AgentChatCoordinatorService(RabbitMQService rabbitMQService, ChatManagementService chatManagementService)
        {
            _rabbitMQService = rabbitMQService;
            _chatManagementService = chatManagementService;
        }

        public void StartListening()
        {
            _channel = _rabbitMQService.Channel;

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var chatSession = JsonConvert.DeserializeObject<ChatSession>(message);
                
                

                // Assign chat session to agent
                await _chatManagementService.AssignChatSession(chatSession);
            };

            _channel.BasicConsume(queue: "ChatSessionsQueue", autoAck: true, consumer: consumer);
        }

        public void StopListening()
        {
            _channel?.Dispose();
            _channel = null;
        }
    }
}