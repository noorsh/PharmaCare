﻿namespace PharmaCare.Services.Implementations
{
    public class ConsultationChatService
    {
        private static readonly List<ChatStep> Steps = new()
        {
            new ChatStep
            {
                StepNumber = 1,
                Question = "What symptoms are you experiencing?",
                InputType = "text",
                Placeholder = "List your symptoms separated by commas — e.g. headache, fever, fatigue, nausea"
            },
            new ChatStep
            {
                StepNumber = 2,
                Question = "How long have you had these symptoms?",
                InputType = "buttons",
                Options = new() { "Less than 24 hours", "1–2 days", "3–7 days", "1–2 weeks", "More than 2 weeks" }
            },
            new ChatStep
            {
                StepNumber = 3,
                Question = "How would you rate the severity of your symptoms?",
                InputType = "buttons",
                Options = new() { "Mild", "Moderate", "Severe" }
            },
            new ChatStep
            {
                StepNumber = 4,
                Question = "Do you have any of these additional symptoms?",
                InputType = "text",
                Placeholder = "e.g. vomiting, chills, skin rash, loss of appetite, chest pain..."
            },
            new ChatStep
            {
                StepNumber = 5,
                Question = "Are you currently taking any medications for this?",
                InputType = "buttons",
                Options = new() { "No medications", "Took a painkiller", "Taking prescribed medication", "Tried home remedies" }
            },
            new ChatStep
            {
                StepNumber = 6,
                Question = "Any other information you'd like the pharmacist to know?",
                InputType = "text",
                Placeholder = "e.g. I'm diabetic, I'm allergic to penicillin, this happened before..."
            }
        };

        private static readonly List<string> EmergencyKeywords = new()
        {
            "chest pain", "can't breathe", "cannot breathe", "difficulty breathing",
            "unconscious", "heavy bleeding", "heart attack", "stroke", "seizure",
            "overdose", "poisoning", "not breathing"
        };

        public ChatStep GetStep(int stepNumber) =>
            Steps.FirstOrDefault(s => s.StepNumber == stepNumber);

        public ChatStep GetFirstStep() => Steps.First();

        public bool IsLastStep(int stepNumber) => stepNumber == Steps.Count;

        public int TotalSteps => Steps.Count;

        public bool IsEmergency(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            var lower = input.ToLower();
            return EmergencyKeywords.Any(k => lower.Contains(k));
        }

        public string BuildSummary(List<StepAnswer> answers)
        {
            var lines = new List<string>();
            foreach (var answer in answers)
            {
                var step = GetStep(answer.StepNumber);
                if (step != null)
                    lines.Add($"• {step.Question}\n  → {answer.Answer}");
            }
            return string.Join("\n\n", lines);
        }
    }

    public class ChatStep
    {
        public int StepNumber { get; set; }
        public string Question { get; set; }
        public string InputType { get; set; } // "text" or "buttons"
        public List<string>? Options { get; set; }
        public string? Placeholder { get; set; }
    }

    public class StepAnswer
    {
        public int StepNumber { get; set; }
        public string? Answer { get; set; }
    }
}