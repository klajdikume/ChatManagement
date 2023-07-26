using RabbitMQ.Client;

namespace ChatManagement.Services
{
    public class RabbitMQService : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        
        public RabbitMQService()
        {
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void CreateQueue(string queueName)
        {
            _channel.QueueDeclare(queue: queueName,
                          durable: false,
                          exclusive: false,
                          autoDelete: false,
                          arguments: null);
        }

        public IModel Channel => _channel;

        public void SendMessage(string queueName, byte[] message)
        {
            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: message);
        }

        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}
