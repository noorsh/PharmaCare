using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCare.Services;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.MVC.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly IConsultationService _consultationService;
        private readonly IEmailService        _emailService;
        private readonly ILogger<MessageController> _logger;

        public MessageController(
            IConsultationService consultationService,
            IEmailService emailService,
            ILogger<MessageController> logger)
        {
            _consultationService = consultationService;
            _emailService        = emailService;
            _logger              = logger;
        }

        // POST: Message/Send
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int consultationId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return BadRequest(new { error = "Message cannot be empty." });

            try
            {
                var userId     = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var senderRole = User.IsInRole("Pharmacist") || User.IsInRole("Admin") ? "Pharmacist" : "Patient";

                var consultation = await _consultationService.GetConsultationWithDetailsAsync(consultationId);
                if (consultation == null) return NotFound();

                await _consultationService.SendMessageAsync(consultationId, userId, senderRole, message);


                // Email notification to the other party
                if (senderRole == "Pharmacist" && consultation.Patient?.User?.Email != null)
                {
                    var patientName = $"{consultation.Patient.User.FirstName} {consultation.Patient.User.LastName}";
                    await _emailService.SendEmailAsync(
                        consultation.Patient.User.Email,
                        patientName,
                        "PharmaCare — Your pharmacist has a question",
                        MessageEmail(
                            patientName,
                            $"{consultation.Pharmacist?.FirstName} {consultation.Pharmacist?.LastName}",
                            message,
                            "Patient",
                            consultationId));
                }
                else if (senderRole == "Patient" && consultation.Pharmacist?.Email != null)
                {
                    var pharmacistName = $"{consultation.Pharmacist.FirstName} {consultation.Pharmacist.LastName}";
                    var patient        = consultation.Patient?.User;
                    var patientName    = patient != null
                        ? $"{patient.FirstName} {patient.LastName}" : "Patient";

                    await _emailService.SendEmailAsync(
                        consultation.Pharmacist.Email,
                        pharmacistName,
                        "PharmaCare — Patient replied to your question",
                        MessageEmail(
                            pharmacistName,
                            patientName,
                            message,
                            "Pharmacist",
                            consultationId));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending message for consultation {consultationId}");
                TempData["Error"] = "An error occurred while sending your message.";
            }

            return RedirectBack(consultationId);
        }

        // GET: Message/GetMessages/5 (polling endpoint)
        [HttpGet]
        public async Task<IActionResult> GetMessages(int consultationId)
        {
            try
            {
                var userId   = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var messages = await _consultationService.GetMessagesAsync(consultationId);

                await _consultationService.MarkMessagesAsReadAsync(consultationId, userId);

                return Json(messages.Select(m => new
                {
                    messageId  = m.MessageId,
                    message    = m.Message,
                    senderRole = m.SenderRole,
                    senderName = $"{m.Sender?.FirstName} {m.Sender?.LastName}",
                    sentAt     = m.SentAt.ToString("MMM dd, HH:mm"),
                    isRead     = m.IsRead
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting messages for consultation {consultationId}");
                return Json(new List<object>());
            }
        }

        // GET: Message/UnreadCount/5
        [HttpGet]
        public async Task<IActionResult> UnreadCount(int consultationId)
        {
            try
            {
                var userId   = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var messages = await _consultationService.GetMessagesAsync(consultationId);
                var count    = messages.Count(m => !m.IsRead && m.SenderUserId != userId);
                return Json(new { count });
            }
            catch
            {
                return Json(new { count = 0 });
            }
        }

        private IActionResult RedirectBack(int consultationId)
        {
            if (User.IsInRole("Patient"))
                return RedirectToAction("Details", "Consultation", new { id = consultationId });
            return RedirectToAction("Review", "Consultation", new { id = consultationId });
        }

        private string MessageEmail(string recipientName, string senderName, string message, string role, int consultationId) => $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:20px;border:1px solid #e0e0e0;border-radius:8px;">
                <h2 style="color:#1392ec;">New Message from {senderName}</h2>
                <p>Hi <strong>{recipientName}</strong>,</p>
                <p>You have a new message regarding your consultation:</p>
                <div style="background:#f0f8ff;border-left:4px solid #1392ec;padding:16px;border-radius:4px;margin:20px 0;">
                    <p style="margin:0;font-style:italic;">"{message}"</p>
                    <p style="margin:8px 0 0;font-size:12px;color:#617789;">— {senderName}</p>
                </div>
                <p>Please log in to PharmaCare to reply.</p>
                <hr style="border:none;border-top:1px solid #e0e0e0;margin:20px 0;" />
                <p style="color:#888;font-size:12px;text-align:center;">PharmaCare — Your health, our priority.</p>
            </div>
            """;
    }
}