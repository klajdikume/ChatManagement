using ChatManagement.Models;

namespace ChatManagement.IServices
{
    public interface IChatManagementService
    {
        bool RefuseChat();
        void IncrementQueueLength();
        ChatSession GetSession(Guid sessionId);
        public Agent GetNextAvailableAgent();
        public void AssignChat(Agent agent, ChatSession chatSession);
        void DecrementQueueLength();
        public Task AssignChatSession(ChatSession chatSession);
        public List<ChatSession> GetAllChatSessions();
        public void AddTeam(Team team);
    }
}
