using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCare.Services;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.MVC.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IConsultationService _consultationService;
        private readonly IEmailService _emailService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IConsultationService consultationService,
            IEmailService emailService,
            ILogger<OrderController> logger)
        {
            _consultationService = consultationService;
            _emailService        = emailService;
            _logger              = logger;
        }

        // POST: Order/Place
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Place(int consultationId, string deliveryAddress)
        {
            if (string.IsNullOrWhiteSpace(deliveryAddress))
            {
                TempData["Error"] = "Please provide a delivery address.";
                return RedirectToAction("Details", "Consultation", new { id = consultationId });
            }

            var userId  = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var success = await _consultationService.PlaceOrderAsync(consultationId, userId, deliveryAddress);

            if (success)
            {
                // Notify pharmacist by email
                var consultation = await _consultationService.GetConsultationWithDetailsAsync(consultationId);
                if (consultation?.Pharmacist?.Email != null)
                {
                    var patientName = $"{consultation.Patient?.User?.FirstName} {consultation.Patient?.User?.LastName}";
                    var medName     = consultation.Recommendation?.Inventory?.MedicineName ?? "medication";

                    await _emailService.SendEmailAsync(
                        consultation.Pharmacist.Email,
                        $"{consultation.Pharmacist.FirstName} {consultation.Pharmacist.LastName}",
                        "PharmaCare — New Medication Order",
                        OrderRequestedEmail(patientName, medName, deliveryAddress, consultationId)
                    );
                }

                TempData["Success"] = "Order placed successfully! The pharmacist will dispatch your medication.";
            }
            else
            {
                TempData["Error"] = "Unable to place order. It may have already been ordered.";
            }

            return RedirectToAction("Details", "Consultation", new { id = consultationId });
        }

        // POST: Order/Dispatch
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Pharmacist,Admin")]
        public async Task<IActionResult> Dispatch(int orderId, int consultationId, string? pharmacistNotes)
        {
            var success = await _consultationService.DispatchOrderAsync(orderId, pharmacistNotes ?? "");

            if (success)
            {
                // Notify patient by email
                var consultation = await _consultationService.GetConsultationWithDetailsAsync(consultationId);
                if (consultation?.Patient?.User?.Email != null)
                {
                    var patientName = $"{consultation.Patient.User.FirstName} {consultation.Patient.User.LastName}";
                    var medName     = consultation.Recommendation?.Inventory?.MedicineName ?? "medication";

                    await _emailService.SendEmailAsync(
                        consultation.Patient.User.Email,
                        patientName,
                        "PharmaCare — Your Medication Has Been Dispatched",
                        OrderDispatchedEmail(patientName, medName, pharmacistNotes)
                    );
                }

                TempData["Success"] = "Order marked as dispatched. Patient has been notified.";
            }
            else
            {
                TempData["Error"] = "Unable to update order status.";
            }

            return RedirectToAction("Details", "Consultation", new { id = consultationId });
        }

        // ─── Email Templates ─────────────────────────────────────────────────

        private string OrderRequestedEmail(string patientName, string medName, string address, int consultationId) => $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;">
                <h2 style="color: #1392ec;">New Medication Order 📦</h2>
                <p>Hi,</p>
                <p><strong>{patientName}</strong> has requested delivery of their prescribed medication.</p>
                <div style="background: #f0f8ff; border-left: 4px solid #1392ec; padding: 16px; border-radius: 4px; margin: 20px 0;">
                    <p style="margin:0"><strong>Medication:</strong> {medName}</p>
                    <p style="margin:8px 0 0"><strong>Delivery Address:</strong> {address}</p>
                </div>
                <p>Please log in to PharmaCare to review and dispatch the order.</p>
                <hr style="border:none; border-top:1px solid #e0e0e0; margin: 20px 0;" />
                <p style="color:#888; font-size:12px; text-align:center;">PharmaCare — Your health, our priority.</p>
            </div>
            """;

        private string OrderDispatchedEmail(string patientName, string medName, string? notes) => $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;">
                <h2 style="color: #10b981;">Medication Dispatched ✓</h2>
                <p>Hi <strong>{patientName}</strong>,</p>
                <p>Your medication has been dispatched and is on its way to you.</p>
                <div style="background: #f0fff8; border-left: 4px solid #10b981; padding: 16px; border-radius: 4px; margin: 20px 0;">
                    <p style="margin:0"><strong>Medication:</strong> {medName}</p>
                    {(string.IsNullOrEmpty(notes) ? "" : $"<p style='margin:8px 0 0'><strong>Note from pharmacist:</strong> {notes}</p>")}
                </div>
                <p>If you have any questions, please contact your pharmacist through PharmaCare.</p>
                <hr style="border:none; border-top:1px solid #e0e0e0; margin: 20px 0;" />
                <p style="color:#888; font-size:12px; text-align:center;">PharmaCare — Your health, our priority.</p>
            </div>
            """;
    }
}