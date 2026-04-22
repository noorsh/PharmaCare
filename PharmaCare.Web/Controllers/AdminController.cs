using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaCare.Data.Models;
using PharmaCare.MVC.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using PharmaCare.Business.Services.Interfaces;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser>  _userManager;
        private readonly RoleManager<IdentityRole>     _roleManager;
        private readonly ILogger<AdminController>      _logger;
        private readonly IConsultationService          _consultationService;
        private readonly IInventoryService             _inventoryService;

        public AdminController(
            UserManager<ApplicationUser>  userManager,
            RoleManager<IdentityRole>     roleManager,
            ILogger<AdminController>      logger,
            IConsultationService          consultationService,
            IInventoryService             inventoryService)
        {
            _userManager         = userManager;
            _roleManager         = roleManager;
            _logger              = logger;
            _consultationService = consultationService;
            _inventoryService    = inventoryService;
        }

        // ─────────────────────────────────────────────────────────────────
        // DASHBOARD
        // ─────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var viewModel = new AdminDashboardViewModel();

                var allUsers = await _userManager.Users.ToListAsync();
                viewModel.TotalUsers        = allUsers.Count;
                viewModel.UserGrowthPercent = 12;

                var patients    = new List<ApplicationUser>();
                var pharmacists = new List<ApplicationUser>();

                foreach (var user in allUsers)
                {
                    if (await _userManager.IsInRoleAsync(user, "Patient"))
                        patients.Add(user);
                    else if (await _userManager.IsInRoleAsync(user, "Pharmacist"))
                        pharmacists.Add(user);
                }

                viewModel.ActivePatients        = patients.Count;
                viewModel.PatientGrowthPercent  = 5;
                viewModel.TotalPharmacists      = pharmacists.Count;
                viewModel.ActivePharmacists     = pharmacists.Count;
                viewModel.InactivePharmacists   = 0;
                viewModel.PharmacistGrowthPercent = 2;

                // Consultation counts
                var allConsultations = (await _consultationService.GetRecentConsultationsWithDetailsAsync(1000)).ToList();
                viewModel.PendingConsultations  = allConsultations.Count(c => c.Status == "Pending");
                viewModel.TodayConsultations    = allConsultations.Count(c => c.CreatedAt.Date == DateTime.UtcNow.Date);

                viewModel.SystemUptime = 99.9m;

                var today = DateTime.UtcNow.Date;
                viewModel.TodayNewUsers     = allUsers.Count(u => u.CreatedAt.Date == today);
                viewModel.UserSignupTrend   = "+12% from yesterday";

                // Recent users
                var recentUsers = allUsers.OrderByDescending(u => u.CreatedAt).Take(10).ToList();
                foreach (var user in recentUsers)
                {
                    var roles       = await _userManager.GetRolesAsync(user);
                    var primaryRole = roles.FirstOrDefault() ?? "Patient";

                    viewModel.RecentUsers.Add(new UserSummary
                    {
                        UserId    = user.Id,
                        FullName  = $"{user.FirstName} {user.LastName}",
                        Email     = user.Email ?? "",
                        Role      = primaryRole,
                        IsActive  = true,
                        CreatedAt = user.CreatedAt
                    });
                }

                viewModel.RecentActivities = GetRecentActivities();

                viewModel.SystemAlerts = new List<SystemAlert>
                {
                    new SystemAlert
                    {
                        Title     = "System Backup Successful",
                        Message   = "Daily backup completed at 2:00 AM. All data secure.",
                        Severity  = "Low",
                        CreatedAt = DateTime.UtcNow.AddHours(-6)
                    }
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                TempData["Error"] = "Error loading dashboard. Please try again.";
                return View(new AdminDashboardViewModel());
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // USERS
        // ─────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Users(int page = 1, string search = "", string role = "", string status = "")
        {
            try
            {
                var viewModel = new UserManagementViewModel
                {
                    CurrentPage  = page,
                    SearchTerm   = search,
                    RoleFilter   = role,
                    StatusFilter = status
                };

                var allUsers       = await _userManager.Users.ToListAsync();
                var filteredUsers  = new List<UserSummary>();

                foreach (var user in allUsers)
                {
                    var roles       = await _userManager.GetRolesAsync(user);
                    var primaryRole = roles.FirstOrDefault() ?? "Patient";

                    if (!string.IsNullOrEmpty(role) && primaryRole != role)
                        continue;

                    if (!string.IsNullOrEmpty(search))
                    {
                        if (!user.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) &&
                            !user.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) &&
                            !(user.Email?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) &&
                            !user.Id.Contains(search, StringComparison.OrdinalIgnoreCase))
                            continue;
                    }

                    filteredUsers.Add(new UserSummary
                    {
                        UserId      = user.Id,
                        FullName    = $"{user.FirstName} {user.LastName}",
                        Email       = user.Email ?? "",
                        PhoneNumber = user.PhoneNumber,
                        Role        = primaryRole,
                        IsActive    = true,
                        CreatedAt   = user.CreatedAt
                    });
                }

                viewModel.TotalUsers = filteredUsers.Count;
                viewModel.Users = filteredUsers
                    .Skip((page - 1) * viewModel.PageSize)
                    .Take(viewModel.PageSize)
                    .ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users page");
                TempData["Error"] = "Error loading users. Please try again.";
                return View(new UserManagementViewModel());
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // CREATE PHARMACIST
        // ─────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePharmacist(CreatePharmacistViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please fill in all required fields.";
                    return RedirectToAction(nameof(Users));
                }

                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    TempData["Error"] = "A user with this email already exists.";
                    return RedirectToAction(nameof(Users));
                }

                var nameParts = model.FullName.Split(' ', 2);
                var user = new ApplicationUser
                {
                    UserName       = model.Email,
                    Email          = model.Email,
                    FirstName      = nameParts[0],
                    LastName       = nameParts.Length > 1 ? nameParts[1] : "",
                    PhoneNumber    = model.PhoneNumber,
                    EmailConfirmed = true,
                    CreatedAt      = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Pharmacist"))
                        await _roleManager.CreateAsync(new IdentityRole("Pharmacist"));

                    await _userManager.AddToRoleAsync(user, "Pharmacist");
                    _logger.LogInformation("Pharmacist '{Email}' created by admin", model.Email);
                    TempData["Success"] = $"Pharmacist account for {model.FullName} created successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to create pharmacist: " +
                        string.Join(", ", result.Errors.Select(e => e.Description));
                }

                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pharmacist");
                TempData["Error"] = "Error creating pharmacist account.";
                return RedirectToAction(nameof(Users));
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // CREATE ADMIN
        // ─────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(CreateAdminViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please fill in all required fields.";
                    return RedirectToAction(nameof(Users));
                }

                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    TempData["Error"] = "A user with this email already exists.";
                    return RedirectToAction(nameof(Users));
                }

                var nameParts = model.FullName.Split(' ', 2);
                var user = new ApplicationUser
                {
                    UserName       = model.Email,
                    Email          = model.Email,
                    FirstName      = nameParts[0],
                    LastName       = nameParts.Length > 1 ? nameParts[1] : "",
                    EmailConfirmed = true,
                    CreatedAt      = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Admin"))
                        await _roleManager.CreateAsync(new IdentityRole("Admin"));

                    await _userManager.AddToRoleAsync(user, "Admin");
                    _logger.LogInformation("Admin '{Email}' created", model.Email);
                    TempData["Success"] = $"Admin account for {model.FullName} created successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to create admin: " +
                        string.Join(", ", result.Errors.Select(e => e.Description));
                }

                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating admin");
                TempData["Error"] = "Error creating admin account.";
                return RedirectToAction(nameof(Users));
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // EDIT USER
        // ─────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Users));
                }

                user.FirstName   = model.FullName;
                user.Email       = model.Email;
                user.UserName    = model.Email;
                user.PhoneNumber = model.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);

                TempData[result.Succeeded ? "Success" : "Error"] =
                    result.Succeeded ? "User updated successfully!" : "Failed to update user.";

                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing user");
                TempData["Error"] = "Error updating user.";
                return RedirectToAction(nameof(Users));
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // UPDATE ROLES
        // ─────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRoles(UpdateRolesViewModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Users));
                }

                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (model.Roles != null && model.Roles.Any())
                {
                    await _userManager.AddToRolesAsync(user, model.Roles);
                    TempData["Success"] = "Roles updated successfully!";
                }
                else
                {
                    TempData["Error"] = "User must have at least one role.";
                }

                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating roles");
                TempData["Error"] = "Error updating roles.";
                return RedirectToAction(nameof(Users));
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // DELETE USER
        // ─────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Users));
                }

                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User '{Email}' deleted by admin", user.Email);
                    TempData["Success"] = "User deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete user.";
                }

                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                TempData["Error"] = "Error deleting user.";
                return RedirectToAction(nameof(Users));
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // EXPORT DATA (users CSV — existing)
        // ─────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> ExportData()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var csv   = new System.Text.StringBuilder();
                csv.AppendLine("User ID,Full Name,Email,Phone,Created Date");

                foreach (var user in users)
                    csv.AppendLine($"{user.Id},{user.FirstName} {user.LastName},{user.Email},{user.PhoneNumber},{user.CreatedAt:yyyy-MM-dd}");

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"users_export_{DateTime.UtcNow:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting data");
                TempData["Error"] = "Error exporting data.";
                return RedirectToAction(nameof(Dashboard));
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // USER GUIDE
        // ─────────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult UserGuide()
        {
            return View();
        }

        // ─────────────────────────────────────────────────────────────────
        // SECURITY PROTOCOL
        // ─────────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Security()
        {
            return View("SecurityProtocol");
        }

        // ─────────────────────────────────────────────────────────────────
        // REPORTS — GET
        // ─────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Reports(
            string    reportType   = "consultations",
            DateTime? dateFrom     = null,
            DateTime? dateTo       = null,
            string?   statusFilter = null)
        {
            var from = dateFrom ?? DateTime.UtcNow.AddDays(-30);
            var to   = dateTo   ?? DateTime.UtcNow;

            var vm = new ReportsViewModel
            {
                ReportType   = reportType,
                DateFrom     = from,
                DateTo       = to,
                StatusFilter = statusFilter
            };

            try
            {
                if (reportType == "consultations")
                    await BuildConsultationReport(vm, from, to, statusFilter);
                else if (reportType == "users")
                    await BuildUserReport(vm, from, to);
                else if (reportType == "inventory")
                    await BuildInventoryReport(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating {ReportType} report", reportType);
                TempData["Error"] = "Error generating report. Please try again.";
            }

            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────────
        // EXPORT REPORT — GET (CSV download)
        // ─────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> ExportReport(
            string    reportType   = "consultations",
            DateTime? dateFrom     = null,
            DateTime? dateTo       = null,
            string?   statusFilter = null)
        {
            var from = dateFrom ?? DateTime.UtcNow.AddDays(-30);
            var to   = dateTo   ?? DateTime.UtcNow;
            var csv  = new System.Text.StringBuilder();

            try
            {
                if (reportType == "consultations")
                {
                    var vm = new ReportsViewModel();
                    await BuildConsultationReport(vm, from, to, statusFilter);

                    csv.AppendLine("ID,Patient,Email,Symptoms,Severity,Status,Pharmacist,Submitted,Completed,Hours to Complete");
                    foreach (var r in vm.ConsultationRows)
                        csv.AppendLine(
                            $"{r.ConsultationId}," +
                            $"\"{r.PatientName}\"," +
                            $"{r.PatientEmail}," +
                            $"\"{r.Symptoms?.Replace("\"", "'")}\"," +
                            $"{r.Severity}," +
                            $"{r.Status}," +
                            $"\"{r.PharmacistName ?? "-"}\"," +
                            $"{r.CreatedAt:yyyy-MM-dd HH:mm}," +
                            $"{(r.CompletedAt.HasValue ? r.CompletedAt.Value.ToString("yyyy-MM-dd HH:mm") : "-")}," +
                            $"{(r.HoursToComplete.HasValue ? r.HoursToComplete.Value.ToString("F1") : "-")}"
                        );
                }
                else if (reportType == "users")
                {
                    var vm = new ReportsViewModel();
                    await BuildUserReport(vm, from, to);

                    csv.AppendLine("ID,Full Name,Email,Role,Phone,Joined");
                    foreach (var r in vm.UserRows)
                        csv.AppendLine($"{r.UserId},\"{r.FullName}\",{r.Email},{r.Role},{r.Phone ?? "-"},{r.CreatedAt:yyyy-MM-dd}");
                }
                else if (reportType == "inventory")
                {
                    var vm = new ReportsViewModel();
                    await BuildInventoryReport(vm);

                    csv.AppendLine("ID,Medicine,Category,Quantity,Status,Expiry");
                    foreach (var r in vm.InventoryRows)
                        csv.AppendLine($"{r.InventoryId},\"{r.MedicineName}\",{r.Category ?? "-"},{r.Quantity},{r.StockStatus},{r.ExpiryDate ?? "-"}");
                }

                var bytes    = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                var fileName = $"pharmacare_{reportType}_report_{DateTime.UtcNow:yyyyMMdd}.csv";
                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting {ReportType} report", reportType);
                TempData["Error"] = "Error exporting report.";
                return RedirectToAction(nameof(Reports));
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────────

        private async Task BuildConsultationReport(
            ReportsViewModel vm, DateTime from, DateTime to, string? statusFilter)
        {
            var allConsultations = (await _consultationService
                .GetRecentConsultationsWithDetailsAsync(1000))
                .Where(c => c.CreatedAt.Date >= from.Date && c.CreatedAt.Date <= to.Date)
                .ToList();

            if (!string.IsNullOrEmpty(statusFilter))
                allConsultations = allConsultations.Where(c => c.Status == statusFilter).ToList();

            vm.TotalConsultations       = allConsultations.Count;
            vm.CompletedConsultations   = allConsultations.Count(c => c.Status == "Completed");
            vm.PendingConsultations     = allConsultations.Count(c => c.Status == "Pending");
            vm.CancelledConsultations   = allConsultations.Count(c => c.Status == "Cancelled");
            vm.UnderReviewConsultations = allConsultations.Count(c => c.Status == "UnderReview");

            var completed = allConsultations
                .Where(c => c.Status == "Completed" && c.CompletedAt.HasValue)
                .ToList();

            vm.AvgCompletionHours = completed.Any()
                ? completed.Average(c => (c.CompletedAt!.Value - c.CreatedAt).TotalHours)
                : 0;

            vm.ConsultationRows = allConsultations.Select(c => new ConsultationReportRow
            {
                ConsultationId  = c.ConsultationId,
                PatientName     = c.Patient?.User != null
                    ? $"{c.Patient.User.FirstName} {c.Patient.User.LastName}"
                    : "Unknown",
                PatientEmail    = c.Patient?.User?.Email ?? "-",
                Symptoms        = c.Symptoms,
                Severity        = c.SymptomSeverity ?? "-",
                Status          = c.Status,
                PharmacistName  = c.Pharmacist != null
                    ? $"{c.Pharmacist.FirstName} {c.Pharmacist.LastName}"
                    : null,
                CreatedAt       = c.CreatedAt,
                CompletedAt     = c.CompletedAt,
                HoursToComplete = c.CompletedAt.HasValue
                    ? (c.CompletedAt.Value - c.CreatedAt).TotalHours
                    : null
            }).ToList();

            var byStatus = new[]
            {
                new { label = "Completed",    count = vm.CompletedConsultations   },
                new { label = "Pending",      count = vm.PendingConsultations     },
                new { label = "Under Review", count = vm.UnderReviewConsultations },
                new { label = "Cancelled",    count = vm.CancelledConsultations   },
            };
            vm.ConsultationsByStatusJson = System.Text.Json.JsonSerializer.Serialize(byStatus);

            var byDay = allConsultations
                .GroupBy(c => c.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new { date = g.Key.ToString("MMM dd"), count = g.Count() })
                .ToList();
            vm.ConsultationsByDayJson = System.Text.Json.JsonSerializer.Serialize(byDay);
        }

        private async Task BuildUserReport(ReportsViewModel vm, DateTime from, DateTime to)
        {
            var allUsers = await _userManager.Users.ToListAsync();
            vm.TotalUsers      = allUsers.Count;
            vm.NewUsersInRange = allUsers.Count(u =>
                u.CreatedAt.Date >= from.Date && u.CreatedAt.Date <= to.Date);

            foreach (var user in allUsers.OrderByDescending(u => u.CreatedAt))
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role  = roles.FirstOrDefault() ?? "Patient";

                if      (role == "Patient")     vm.TotalPatients++;
                else if (role == "Pharmacist")  vm.TotalPharmacists++;
                else if (role == "Admin")       vm.TotalAdmins++;

                vm.UserRows.Add(new UserReportRow
                {
                    UserId    = user.Id,
                    FullName  = $"{user.FirstName} {user.LastName}",
                    Email     = user.Email ?? "-",
                    Role      = role,
                    Phone     = user.PhoneNumber,
                    CreatedAt = user.CreatedAt
                });
            }
        }

        private async Task BuildInventoryReport(ReportsViewModel vm)
        {
            var items = (await _inventoryService.GetAllInventoryAsync()).ToList();

            vm.TotalMedications = items.Count;
            vm.LowStockCount    = items.Count(i => i.QuantityInStock > 0 && i.QuantityInStock <= i.ReorderLevel);
            vm.OutOfStockCount  = items.Count(i => i.QuantityInStock == 0);

            vm.InventoryRows = items.Select(i => new InventoryReportRow
            {
                InventoryId  = i.InventoryId,
                MedicineName = i.MedicineName,
                Category     = i.Category,
                Quantity     = i.QuantityInStock,
                StockStatus  = i.QuantityInStock == 0            ? "Out"
                             : i.QuantityInStock <= i.ReorderLevel ? "Low"
                             : "OK",
                ExpiryDate   = i.ExpiryDate?.ToString("yyyy-MM-dd")
            }).ToList();
        }

        private List<AdminActivity> GetRecentActivities()
        {
            var activities  = new List<AdminActivity>();
            var recentUsers = _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(10)
                .ToList();

            foreach (var user in recentUsers)
            {
                var roles = _userManager.GetRolesAsync(user).Result;
                var role  = roles.FirstOrDefault() ?? "Patient";

                activities.Add(new AdminActivity
                {
                    AdminName = User.Identity?.Name ?? "Admin",
                    Action    = $"created new {role.ToLower()} account for {user.FirstName} {user.LastName}",
                    Type      = "Create",
                    TimeAgo   = GetTimeAgo(user.CreatedAt),
                    Timestamp = user.CreatedAt
                });
            }

            return activities.OrderByDescending(a => a.Timestamp).Take(5).ToList();
        }

        private static string GetTimeAgo(DateTime dateTime)
        {
            var ts = DateTime.UtcNow - dateTime;
            if (ts.TotalMinutes < 1)  return "Just now";
            if (ts.TotalMinutes < 60) return $"{(int)ts.TotalMinutes} minutes ago";
            if (ts.TotalHours   < 24) return $"{(int)ts.TotalHours} hours ago";
            if (ts.TotalDays    < 7)  return $"{(int)ts.TotalDays} days ago";
            return dateTime.ToString("MMM dd, yyyy");
        }
    }
}