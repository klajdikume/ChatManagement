using ChatManagement.IServices;

namespace ChatManagement.Services
{
    public class SessionMonitorService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;

        public SessionMonitorService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(CheckSessionActivity, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(1)); // Adjust the interval as needed

            return Task.CompletedTask;
        }

        private void CheckSessionActivity(object state)
        {
            using var scope = _scopeFactory.CreateScope();
            var chatManagementService = scope.ServiceProvider.GetRequiredService<IChatManagementService>();

            var now = DateTime.UtcNow;

            // Get the chat sessions from all the agents in all teams
            var chatSessions = chatManagementService.GetAllChatSessions();

            foreach (var chatSession in chatSessions)
            {
                
                if (chatSession.PollCount < 3)
                {
                    chatSession.IsActive = false;
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
