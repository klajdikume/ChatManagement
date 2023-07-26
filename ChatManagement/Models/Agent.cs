namespace ChatManagement.Models
{
    public enum AgentSeniority
    {
        Junior = 1,
        MidLevel = 2,
        Senior = 3,
        TeamLead = 4
    }
   
    public class Agent
    {
        public int Id { get; set; }
        public AgentSeniority Seniority { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime ShiftEndsAt { get; set; }
        public List<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();
        public double SeniorityCoef
        {
            get
            {
                return Seniority switch
                {
                    AgentSeniority.Junior => 0.4,
                    AgentSeniority.MidLevel => 0.6,
                    AgentSeniority.Senior => 1.8,
                    AgentSeniority.TeamLead => 0.5,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        public void AssignChatSession(ChatSession chatSession)
        {
            ChatSessions.Add(chatSession);
            
        }

        public void ConsumeChatSession(ChatSession chatSession)
        {
            ChatSessions.Remove(chatSession);

        }
    }
}
