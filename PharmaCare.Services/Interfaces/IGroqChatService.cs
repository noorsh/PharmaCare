namespace PharmaCare.Services.Interfaces
{
    public class GroqMessage
    {
        public string Role { get; set; } = "";    // "user" | "assistant" | "system"
        public string Content { get; set; } = "";
    }

    public class GroqChatResult
    {
        public string Reply { get; set; } = "";
        public bool IsComplete { get; set; } = false;
        public bool IsEmergency { get; set; } = false;
        public ConsultationSummary? Summary { get; set; }
    }

    public class ConsultationSummary
    {
        public string Symptoms { get; set; } = "";
        public string Duration { get; set; } = "";
        public string Severity { get; set; } = "Moderate";
        public string AdditionalInfo { get; set; } = "";
        public string Transcript { get; set; } = "";
    }

    public interface IGroqChatService
    {
        Task<GroqChatResult> SendMessageAsync(List<GroqMessage> history, string userMessage);
    }
}