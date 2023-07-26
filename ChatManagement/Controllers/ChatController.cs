using ChatManagement.IServices;
using ChatManagement.Models;
using ChatManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace ChatManagement.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly RabbitMQService _rabbitMQService;
        private readonly IChatManagementService _chatManagementService;

        public ChatController(RabbitMQService rabbitMQService, IChatManagementService chatManagementService)
        {
            _rabbitMQService = rabbitMQService;
            _chatManagementService = chatManagementService;
        }

        [HttpPost]
        public IActionResult CreateSession()
        {
            if (_chatManagementService.RefuseChat() )
            {
                return BadRequest("No Agents available at the moment.");
            }

            var chatSession = new ChatSession() { SessionId = Guid.NewGuid() };

            var message = JsonConvert.SerializeObject(chatSession);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            _chatManagementService.IncrementQueueLength();
            _rabbitMQService.SendMessage("ChatSessionsQueue", messageBytes);

            return Ok(chatSession.SessionId);
        }

        [HttpGet("poll/{sessionId}")]
        public IActionResult Poll(Guid sessionId)
        {
            var chatSession = _chatManagementService.GetSession(sessionId);

            if (chatSession == null)
            {
                return NotFound();
            }

            if (!chatSession.IsActive)
            {
                return BadRequest("Session is no longer active");
            }

            chatSession.PollCount++;

            return Ok();
        }
    }
}
