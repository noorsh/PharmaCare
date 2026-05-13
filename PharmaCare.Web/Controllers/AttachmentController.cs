using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.MVC.Controllers
{
    [Authorize]
    public class AttachmentController : Controller
    {
        private readonly IConsultationService _consultationService;
        private readonly ILogger<AttachmentController> _logger;
        private readonly IWebHostEnvironment _env;

        public AttachmentController(
            IConsultationService consultationService,
            IWebHostEnvironment env,
            ILogger<AttachmentController> logger)
        {
            _consultationService = consultationService;
            _env                 = env;
            _logger              = logger;
        }

        private string AppDataPath => Path.Combine(_env.ContentRootPath, "App_Data");

        // ── POST: Attachment/Upload ───────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(int consultationId, IFormFile file, string fileType = "Other")
        {
            try
            {
                // Verify access
                var consultation = await _consultationService.GetConsultationWithDetailsAsync(consultationId);
                if (consultation == null) return NotFound();

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Patient can only upload to their own consultations
                if (User.IsInRole("Patient") &&
                    consultation.Patient?.UserId != userId)
                    return Forbid();

                // Pharmacist can only upload to assigned consultations
                if (User.IsInRole("Pharmacist") &&
                    consultation.PharmacistId != userId)
                    return Forbid();

                if (file == null || file.Length == 0)
                {
                    TempData["Error"] = "Please select a file to upload.";
                    return RedirectToAction("Details", "Consultation", new { id = consultationId });
                }

                var attachment = await _consultationService.SaveAttachmentAsync(
                    consultationId, userId, file, fileType, AppDataPath);

                if (attachment == null)
                {
                    TempData["Error"] = "Invalid file. Allowed types: JPG, PNG, PDF, DOCX. Max size: 10MB.";
                    return RedirectToAction("Details", "Consultation", new { id = consultationId });
                }

                // Mark any pending request as fulfilled
                var attachments = await _consultationService.GetAttachmentsAsync(consultationId);
                var pendingRequest = attachments
                    .FirstOrDefault(a => a.FileType == "Requested" && !a.IsRequestFulfilled);

                if (pendingRequest != null)
                    await _consultationService.FulfillAttachmentRequestAsync(pendingRequest.AttachmentId);

                TempData["Success"] = "File uploaded successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading attachment for consultation {consultationId}");
                TempData["Error"] = "An error occurred while uploading the file.";
            }

            // Redirect back to correct page based on role
            if (User.IsInRole("Patient"))
                return RedirectToAction("Details", "Consultation", new { id = consultationId });

            return RedirectToAction("Review", "Consultation", new { id = consultationId });
        }

        // ── GET: Attachment/View/5 ────────────────────────────────────
        // Secure file serving — never exposes raw file path
        public async Task<IActionResult> View(int id)
        {
            try
            {
                var attachment = await _consultationService.GetAttachmentByIdAsync(id);
                if (attachment == null) return NotFound();

                // Skip pending requests (no real file)
                if (attachment.FileType == "Requested") return NotFound();

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var consultation = await _consultationService
                    .GetConsultationWithDetailsAsync(attachment.ConsultationId);

                if (consultation == null) return NotFound();

                // Access check — patient owns it OR pharmacist assigned to it OR admin
                var isPatient    = consultation.Patient?.UserId == userId;
                var isPharmacist = consultation.PharmacistId == userId;
                var isAdmin      = User.IsInRole("Admin");

                if (!isPatient && !isPharmacist && !isAdmin)
                    return Forbid();

                if (!System.IO.File.Exists(attachment.FilePath))
                    return NotFound();

                var fileBytes = await System.IO.File.ReadAllBytesAsync(attachment.FilePath);
                return File(fileBytes, attachment.MimeType, attachment.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error viewing attachment {id}");
                return NotFound();
            }
        }

        // ── POST: Attachment/Delete/5 ─────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int consultationId)
        {
            try
            {
                var userId  = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var success = await _consultationService.DeleteAttachmentAsync(id, userId, AppDataPath);

                TempData[success ? "Success" : "Error"] = success
                    ? "Attachment deleted."
                    : "Unable to delete. You can only delete your own uploads.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting attachment {id}");
                TempData["Error"] = "An error occurred while deleting the file.";
            }

            if (User.IsInRole("Patient"))
                return RedirectToAction("Details", "Consultation", new { id = consultationId });

            return RedirectToAction("Review", "Consultation", new { id = consultationId });
        }

        // ── POST: Attachment/Request ──────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Pharmacist,Admin")]
        public async Task<IActionResult> Request(int consultationId, string requestNote)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(requestNote))
                {
                    TempData["Error"] = "Please describe what you need from the patient.";
                    return RedirectToAction("Review", "Consultation", new { id = consultationId });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _consultationService.RequestAttachmentAsync(consultationId, userId, requestNote);

                TempData["Success"] = "Document request sent to patient.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error requesting attachment for consultation {consultationId}");
                TempData["Error"] = "An error occurred while sending the request.";
            }

            return RedirectToAction("Review", "Consultation", new { id = consultationId });
        }
    }
}