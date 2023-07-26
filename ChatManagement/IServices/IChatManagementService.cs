using ChatManagement.Models;

namespace ChatManagement.IServices
{
    public interface IChatManagementService
    {
        bool RefuseChat();
        void IncrementQueueLength();
        ChatSession GetSession(Guid sessionId);
    }
}
