using ChatManagement.Models;
using ChatManagement.Services;
using Moq;

namespace ChatManagement.Test
{
    public class ChatManagementServiceTests
    {
        [Fact]
        public void ShouldAcceptChatWhenQueueFullButAvailableOverflowAgents()
        {
            var rabbitMQServiceMock = new Mock<RabbitMQService>();
            var service = new ChatManagementService(rabbitMQServiceMock.Object);

            service.AddTeam(new Team
            {
                Id = 1,
                Name = "Team A",
                Agents = new List<Agent>() { new Agent { Id = 1, Seniority = AgentSeniority.TeamLead, IsAvailable = true }, new Agent { Id = 2, Seniority = AgentSeniority.MidLevel, IsAvailable = true }, new Agent { Id = 2, Seniority = AgentSeniority.Junior, IsAvailable = true } },
                ShiftStart = DateTime.Now,
                ShiftEnd = DateTime.Now.AddHours(8)
            });
            service.AddTeam(new Team { Id = 2, Name = "Team B", Agents = new List<Agent>() { new Agent { Id = 1, Seniority = AgentSeniority.Senior, IsAvailable = true }, new Agent { Id = 2, Seniority = AgentSeniority.MidLevel, IsAvailable = true }, new Agent { Id = 3, Seniority = AgentSeniority.Junior, IsAvailable = true }, new Agent { Id = 4, Seniority = AgentSeniority.Junior, IsAvailable = true } }, ShiftStart = DateTime.Now.AddHours(8), ShiftEnd = DateTime.Now.AddHours(16) });
            service.AddTeam(new Team { Id = 3, Name = "Team C", Agents = new List<Agent>() { new Agent { Id = 1, Seniority = AgentSeniority.MidLevel, IsAvailable = true }, new Agent { Id = 2, Seniority = AgentSeniority.MidLevel, IsAvailable = true } }, ShiftStart = DateTime.Now.AddHours(16), ShiftEnd = DateTime.Now.AddHours(24) });
            
            service.AddTeam(new Team { Id = 4, Name = "overflow", Agents = new List<Agent>() { new Agent { Id = 1, Seniority = AgentSeniority.Junior, IsAvailable = true }, new Agent { Id = 2, Seniority = AgentSeniority.Junior, IsAvailable = true }, new Agent { Id = 3, Seniority = AgentSeniority.Junior, IsAvailable = true } }, });

            // capacity queue is 22
            for (int i = 0; i < 23; i++)
            {
                service.IncrementQueueLength();
            }

            bool result = service.RefuseChat();

            Assert.False(result);
        }

        [Fact]
        public void ShouldRefuseChatWhenQueueFullButNotAvailableOverflowAgents()
        {
            var rabbitMQServiceMock = new Mock<RabbitMQService>();
            var service = new ChatManagementService(rabbitMQServiceMock.Object);

            service.AddTeam(new Team
            {
                Id = 1,
                Name = "Team A",
                Agents = new List<Agent>() { new Agent { Id = 1, Seniority = AgentSeniority.TeamLead, IsAvailable = true }, new Agent { Id = 2, Seniority = AgentSeniority.MidLevel, IsAvailable = true }, new Agent { Id = 2, Seniority = AgentSeniority.Junior, IsAvailable = true } },
                ShiftStart = DateTime.Now,
                ShiftEnd = DateTime.Now.AddHours(8)
            });
            service.AddTeam(new Team { Id = 2, Name = "Team B", Agents = new List<Agent>() { new Agent { Id = 1, Seniority = AgentSeniority.Senior, IsAvailable = true }, new Agent { Id = 2, Seniority = AgentSeniority.MidLevel, IsAvailable = true }, new Agent { Id = 3, Seniority = AgentSeniority.Junior, IsAvailable = true }, new Agent { Id = 4, Seniority = AgentSeniority.Junior, IsAvailable = true } }, ShiftStart = DateTime.Now.AddHours(8), ShiftEnd = DateTime.Now.AddHours(16) });
            service.AddTeam(new Team { Id = 3, Name = "Team C", Agents = new List<Agent>() { new Agent { Id = 1, Seniority = AgentSeniority.MidLevel, IsAvailable = true }, new Agent { Id = 2, Seniority = AgentSeniority.MidLevel, IsAvailable = true } }, ShiftStart = DateTime.Now.AddHours(16), ShiftEnd = DateTime.Now.AddHours(24) });

            service.AddTeam(new Team { Id = 4, Name = "overflow", Agents = new List<Agent>() { new Agent { Id = 1, Seniority = AgentSeniority.Junior, IsAvailable = false }, new Agent { Id = 2, Seniority = AgentSeniority.Junior, IsAvailable = false }, new Agent { Id = 3, Seniority = AgentSeniority.Junior, IsAvailable = false } }, });
            
            // capacity queue is 22
            for (int i = 0; i < 23; i++)
            {
                service.IncrementQueueLength();
            }

            bool result = service.RefuseChat();

            Assert.True(result);
        }
    }
}