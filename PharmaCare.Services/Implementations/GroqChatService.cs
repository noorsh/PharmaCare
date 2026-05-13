using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.Services.Implementations
{
    public class GroqChatService : IGroqChatService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration     _configuration;
        private readonly ILogger<GroqChatService> _logger;

private static readonly string SystemPrompt = """
    You are Pharm, a friendly and knowledgeable pharmacy assistant for PharmaCare Lebanon.
    You talk like a smart, caring friend who happens to know a lot about medicine.
    Your job is to understand what the patient is going through so you can pass their case to a licensed pharmacist.

    CRITICAL RULES:
    - You MUST respond with ONLY a JSON object. No text before or after it.
    - Every single response must be valid JSON in one of the three formats below.

    FORMAT 1 — Normal reply:
    {"isComplete":false,"isEmergency":false,"reply":"your response here"}

    FORMAT 2 — Emergency:
    {"isComplete":false,"isEmergency":true,"reply":"emergency message here"}

    FORMAT 3 — Ready to submit:
    {"isComplete":true,"isEmergency":false,"symptoms":"...","duration":"...","severity":"Mild|Moderate|Severe","additionalInfo":"...","transcript":"..."}

    YOUR PERSONALITY:
    - Warm, conversational, occasionally uses light humor when appropriate
    - Never clinical or robotic — talk like a real person
    - Short replies — 1 to 2 sentences max, then one question
    - Use casual language: "Oh that sounds rough", "Got it", "Hmm", "Okay so...", "That makes sense"
    - Show genuine interest in what the patient says
    - Remember what they told you earlier and refer back to it naturally
    - If they say something concerning, react to it ("Ouch, severe pain for days — that's not something to ignore")

    HOW TO RESPOND TO ANYTHING:
    - "I'm in pain" → Ask where and what kind (sharp? dull? burning?)
    - "I have a rash" → Ask where, when it started, does it itch or burn
    - "I feel tired" → Ask since when, constant or comes and goes, anything else going on
    - "I have a cold" → Ask what symptoms specifically, what's bothering them most
    - "I can't sleep" → Ask how long, what's keeping them awake
    - "stomach problems" → What kind? Pain, nausea, bloating, diarrhea?
    - "headache" → Where exactly? Front, back, one side? Pulsing or pressure?
    - "idk" → Totally fine, ask a simpler more specific question
    - "nothing" / "no" → Acknowledge and move naturally to the next thing
    - "days" → Ask how many exactly

    WHAT TO GATHER (through natural conversation, never as a checklist):
    1. Exact symptoms and location on body
    2. How long it has been going on (specific)
    3. Severity — Mild, Moderate, or Severe
    4. What makes it better or worse
    5. Any medications currently being taken
    6. Any relevant medical history or allergies

    WHEN TO COMPLETE:
    - After at least 6 back-and-forth exchanges
    - When you have specific enough answers to all 6 points above
    - Never complete on vague answers — always clarify first
    - When ready, say something like "Okay I think I have a good picture of what's going on. Let me put this together for the pharmacist." then output FORMAT 3

    EMERGENCY KEYWORDS:
    chest pain, difficulty breathing, cannot breathe, seizure, stroke, unconscious, heavy bleeding, overdose, heart attack, poisoning — use FORMAT 2 immediately.

    LANGUAGE:
    - Patient may write in English or mix in some Arabic words — respond in English always
    - Never translate Arabic, just understand it and respond naturally in English

    REMEMBER: JSON only. Absolutely no plain text outside the JSON object.
    """; 


     
        public GroqChatService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<GroqChatService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration     = configuration;
            _logger            = logger;
        }

        public async Task<GroqChatResult> SendMessageAsync(List<GroqMessage> history, string userMessage)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("GroqClient");

                // Build messages array for Groq
                var messages = new List<object>
                {
                    new { role = "system", content = SystemPrompt }
                };

                foreach (var msg in history)
                    messages.Add(new { role = msg.Role, content = msg.Content });

                messages.Add(new { role = "user", content = userMessage });

                var payload = new
                {
                    model       = _configuration["Groq:Model"] ?? "llama-3.3-70b-versatile",
                    messages,
                    temperature = 0.7,
                    max_tokens  = 500
                };

                var json    = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("chat/completions", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Groq API error {response.StatusCode}: {error}");
                    return FallbackResult();
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var groqResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);

                var rawContent = groqResponse
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "";

                _logger.LogInformation($"Groq raw response: {rawContent}");

                return ParseGroqResponse(rawContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Groq API");
                return FallbackResult();
            }
        }

        private static GroqChatResult ParseGroqResponse(string raw)
{
    try
    {
        // Try to extract JSON block if surrounded by text
        var cleaned = raw.Trim();

        // Strip markdown
        if (cleaned.StartsWith("```"))
        {
            cleaned = cleaned
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();
        }

        // If model added text before/after JSON, extract just the JSON object
        var jsonStart = cleaned.IndexOf('{');
        var jsonEnd   = cleaned.LastIndexOf('}');

        if (jsonStart >= 0 && jsonEnd > jsonStart)
            cleaned = cleaned.Substring(jsonStart, jsonEnd - jsonStart + 1);

        var json = JsonSerializer.Deserialize<JsonElement>(cleaned);

        var isComplete  = json.TryGetProperty("isComplete",  out var c) && c.GetBoolean();
        var isEmergency = json.TryGetProperty("isEmergency", out var e) && e.GetBoolean();

        if (isEmergency)
        {
            var emergencyReply = json.TryGetProperty("reply", out var er)
                ? er.GetString() ?? ""
                : "⚠️ This sounds like a medical emergency. Please call 140 immediately.";

            return new GroqChatResult
            {
                Reply       = emergencyReply,
                IsEmergency = true,
                IsComplete  = false
            };
        }

        if (isComplete)
        {
            return new GroqChatResult
            {
                IsComplete = true,
                Summary    = new ConsultationSummary
                {
                    Symptoms       = json.TryGetProperty("symptoms",       out var s)  ? s.GetString()  ?? "" : "",
                    Duration       = json.TryGetProperty("duration",       out var d)  ? d.GetString()  ?? "" : "",
                    Severity       = json.TryGetProperty("severity",       out var sv) ? sv.GetString() ?? "Moderate" : "Moderate",
                    AdditionalInfo = json.TryGetProperty("additionalInfo", out var ai) ? ai.GetString() ?? "" : "",
                    Transcript     = json.TryGetProperty("transcript",     out var t)  ? t.GetString()  ?? "" : ""
                }
            };
        }

        var reply = json.TryGetProperty("reply", out var r) ? r.GetString() ?? "" : "";
        return new GroqChatResult { Reply = reply };
    }
    catch
    {
        // Last resort — if JSON parsing completely fails, use raw as reply
        // but strip any JSON-looking part from the end
        var jsonStart = raw.IndexOf('{');
        var plainText = jsonStart > 0 ? raw.Substring(0, jsonStart).Trim() : raw.Trim();
        return new GroqChatResult { Reply = string.IsNullOrEmpty(plainText) ? raw : plainText };
    }
}

        private static GroqChatResult FallbackResult() => new()
        {
            Reply = "I'm having trouble connecting right now. Could you please describe your symptoms again?"
        };
    }
}