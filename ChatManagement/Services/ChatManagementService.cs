using ChatManagement.IServices;
using ChatManagement.Models;
using System.Numerics;

namespace ChatManagement.Services
{
    public class ChatManagementService : IChatManagementService
    {
        private readonly List<Team> _teams = new List<Team>();
        private readonly RabbitMQService _rabbitMQService;
        private int _currentQueueLength = 0;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public ChatManagementService(RabbitMQService rabbitMQService)
        {
            _rabbitMQService = rabbitMQService;
        }

        public void IncrementQueueLength()
        {
            _currentQueueLength++;
        }

        public void DecrementQueueLength()
        {
            _currentQueueLength--;
        }

        public void AddTeam(Team team)
        {
            _teams.Add(team);
        }

        public int GetMaxQueueLength()
        {
            double capacity = 0;
            DateTime now = DateTime.Now;
            foreach (var team in _teams)
            {
                if (now >= team.ShiftStart && now <= team.ShiftEnd)
                {
                    foreach (var agent in team.Agents)
                    {
                        capacity += (agent.SeniorityCoef * 10);
                    }
                }
            }

            int queueSize = (int)(capacity * 1.5);
            return queueSize;
        }

        public bool RefuseChat()
        {
            if (IsQueueFull())
            {
                var overflowTeam = _teams.FirstOrDefault(t => t.Name.Equals("overflow"));
                if (overflowTeam is null || !overflowTeam.Agents.Any(a => a.IsAvailable))
                {
                    return true;
                }
            }
            
            return false;
        }

        public bool IsQueueFull()
        {
            return _currentQueueLength >= GetMaxQueueLength();
        }
        public List<ChatSession> GetAllChatSessions()
        {
            return _teams.SelectMany(t => t.Agents.SelectMany(a => a.ChatSessions)).ToList();
        }

        public ChatSession GetSession(Guid sessionId)
        {
            return _teams.SelectMany(t => t.Agents)
                         .SelectMany(a => a.ChatSessions)
                         .FirstOrDefault(s => s.SessionId == sessionId);
        }

        public void AssignChat(Agent agent, ChatSession chatSession)
        {
            agent.ChatSessions.Add(chatSession);
        }

        public async Task AssignChatSession(ChatSession chatSession)
        {
            await _semaphore.WaitAsync();
            try
            {
                var agent = GetNextAvailableAgent();

                if (agent != null)
                {
                    // Assign the chat session to the agent
                    agent.AssignChatSession(chatSession);
                    agent.IsAvailable= false;
                    // Log the assignment
                    Console.WriteLine($"Assigned chat session {chatSession.SessionId} to agent {agent.Id}");
                    
                    
                    agent.IsAvailable = true;
                    DecrementQueueLength();
                    Console.WriteLine($" {chatSession.SessionId} - consumed by {agent.Id}");
                }
                else
                {
                    // No available agents, refuse the chat
                    Console.WriteLine($"No available agents to handle chat session {chatSession.SessionId}");
                }
            }
            finally
            {
                _semaphore.Release();
            }
            
        }  

        public Agent GetNextAvailableAgent()
        {

            if (IsQueueFull())
            {
                var overflowTeam = _teams.FirstOrDefault(t => t.Name.Equals("overflow"));
                var overflowAgent = overflowTeam.Agents.FirstOrDefault(t => t.IsAvailable);
                if (overflowAgent is null)
                    throw new Exception("There is no agent available at the moment!");

                return overflowAgent;
            }

            // Sort the agents based on their seniority, but only consider agents currently in shift
            var agentsInShift = _teams
                .Where(team => DateTime.Now > team.ShiftStart && DateTime.Now < team.ShiftEnd)
                .SelectMany(team => team.Agents)
                .OrderBy(agent => agent.Seniority);

            // Find the next available agent
            var agent = agentsInShift.FirstOrDefault(agent => agent.IsAvailable);

            return agent;
        }
    }
}

