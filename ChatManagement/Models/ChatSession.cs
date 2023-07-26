namespace ChatManagement.Models
{
    public class ChatSession
    {
        public Guid SessionId { get; set; }
        public int PollCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;  
    }
}
