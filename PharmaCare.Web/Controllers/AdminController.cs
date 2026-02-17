using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaCare.Data.Models;
using PharmaCare.MVC.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace PharmaCare.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var viewModel = new AdminDashboardViewModel();

                // Get all users
                var allUsers = await _userManager.Users.ToListAsync();
                var totalUsers = allUsers.Count;

                // Calculate statistics
                viewModel.TotalUsers = totalUsers;
                viewModel.UserGrowthPercent = 12; // TODO: Calculate actual growth

                // Get patients
                var patients = new List<ApplicationUser>();
                foreach (var user in allUsers)
                {
                    if (await _userManager.IsInRoleAsync(user, "Patient"))
                    {
                        patients.Add(user);
                    }
                }
                viewModel.ActivePatients = patients.Count;
                viewModel.PatientGrowthPercent = 5; // TODO: Calculate actual growth

                // Get pharmacists
                var pharmacists = new List<ApplicationUser>();
                foreach (var user in allUsers)
                {
                    if (await _userManager.IsInRoleAsync(user, "Pharmacist"))
                    {
                        pharmacists.Add(user);
                    }
                }
                viewModel.TotalPharmacists = pharmacists.Count;
                viewModel.ActivePharmacists = pharmacists.Count; // TODO: Check actual active status
                viewModel.InactivePharmacists = 0; // TODO: Calculate
                viewModel.PharmacistGrowthPercent = 2;

                // Pending consultations (mock data for now)
                viewModel.PendingConsultations = 0; // TODO: Get from Consultation table when created

                // System health
                viewModel.SystemUptime = 99.9m;

                // Today's activity
                var today = DateTime.UtcNow.Date;
                viewModel.TodayNewUsers = allUsers.Count(u => u.CreatedAt.Date == today);
                viewModel.UserSignupTrend = "+12% from yesterday";
                viewModel.TodayConsultations = 0; // TODO: Get from Consultation table

                // Recent users (last 10)
                var recentUsers = allUsers
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(10)
                    .ToList();

                foreach (var user in recentUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var primaryRole = roles.FirstOrDefault() ?? "Patient";

                    viewModel.RecentUsers.Add(new UserSummary
                    {
                        UserId = user.Id,
                        FullName = user.FirstName + " " + user.LastName,
                        Email = user.Email ?? "",
                        Role = primaryRole,
                        IsActive = true, // TODO: Implement actual active status
                        CreatedAt = user.CreatedAt
                    });
                }

                // Recent admin activities 
                viewModel.RecentActivities = GetRecentActivities();
                // System alerts (mock data)
                viewModel.SystemAlerts = new List<SystemAlert>
                {
                    new SystemAlert
                    {
                        Title = "System Backup Successful",
                        Message = "Daily backup completed at 2:00 AM. All data secure.",
                        Severity = "Low",
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
        private List<AdminActivity> GetRecentActivities()
        {
            var activities = new List<AdminActivity>();
    
            // Get recent user creations
            var recentUsers = _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(10)
                .ToList();
    
            foreach (var user in recentUsers)
            {
                var roles = _userManager.GetRolesAsync(user).Result;
                var role = roles.FirstOrDefault() ?? "Patient";
        
                var timeAgo = GetTimeAgo(user.CreatedAt);
        
                activities.Add(new AdminActivity
                {
                    AdminName = User.Identity.Name, // You can track who created it if needed
                    Action = $"created new {role.ToLower()} account for {user.FirstName}  {user.LastName}",
                    Type = "Create",
                    TimeAgo = timeAgo,
                    Timestamp = user.CreatedAt
                });
            }
    
            return activities.OrderByDescending(a => a.Timestamp).Take(5).ToList();
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;
    
            if (timeSpan.TotalMinutes < 1) return "Just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays} days ago";
            return dateTime.ToString("MMM dd, yyyy");
        }
        [HttpGet]
        public async Task<IActionResult> Users(int page = 1, string search = "", string role = "", string status = "")
        {
            try
            {
                var viewModel = new UserManagementViewModel
                {
                    CurrentPage = page,
                    SearchTerm = search,
                    RoleFilter = role,
                    StatusFilter = status
                };

                // Get all users
                var allUsers = await _userManager.Users.ToListAsync();

                // Apply filters
                var filteredUsers = new List<UserSummary>();
                
                foreach (var user in allUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var primaryRole = roles.FirstOrDefault() ?? "Patient";
                    
                    // Apply role filter
                    if (!string.IsNullOrEmpty(role) && primaryRole != role)
                        continue;
                    
                    // Apply search filter
                    if (!string.IsNullOrEmpty(search))
                    {
                        if (!user.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) &&
                            !user.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) &&
                            !(user.Email?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) &&
                            !user.Id.Contains(search, StringComparison.OrdinalIgnoreCase))
                            continue;
                    }
                    
                    // Apply status filter (for now all are active, implement later)
                    if (!string.IsNullOrEmpty(status))
                    {
                        // TODO: Implement actual status check
                    }

                    filteredUsers.Add(new UserSummary
                    {
                        UserId = user.Id,
                        FullName = user.FirstName + " " + user.LastName,
                        Email = user.Email ?? "",
                        PhoneNumber = user.PhoneNumber,
                        Role = primaryRole,
                        IsActive = true,
                        CreatedAt = user.CreatedAt
                    });
                }

                viewModel.TotalUsers = filteredUsers.Count;

                // Apply pagination
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

                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    TempData["Error"] = "A user with this email already exists.";
                    return RedirectToAction(nameof(Users));
                }

                // Create user
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FullName.Split(' ')[0],
                    LastName = model.FullName.Split(' ')[1],
                    PhoneNumber = model.PhoneNumber,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Ensure Pharmacist role exists
                    if (!await _roleManager.RoleExistsAsync("Pharmacist"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Pharmacist"));
                    }

                    // Assign Pharmacist role
                    await _userManager.AddToRoleAsync(user, "Pharmacist");

                    // TODO: Save license number to Pharmacist table

                    _logger.LogInformation($"Pharmacist user '{model.Email}' created by admin");
                    TempData["Success"] = $"Pharmacist account for {model.FullName} created successfully!";
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    TempData["Error"] = "Failed to create pharmacist: " + errors;
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

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FullName.Split(' ')[0],
                    LastName = model.FullName.Split(' ')[1],
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Admin"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    }

                    await _userManager.AddToRoleAsync(user, "Admin");

                    _logger.LogInformation($"Admin user '{model.Email}' created");
                    TempData["Success"] = $"Admin account for {model.FullName} created successfully!";
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    TempData["Error"] = "Failed to create admin: " + errors;
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

                user.FirstName = model.FullName;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.PhoneNumber = model.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    TempData["Success"] = "User updated successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to update user.";
                }

                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing user");
                TempData["Error"] = "Error updating user.";
                return RedirectToAction(nameof(Users));
            }
        }

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

                // Get current roles
                var currentRoles = await _userManager.GetRolesAsync(user);

                // Remove all current roles
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                // Add new roles
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
                    _logger.LogInformation($"User '{user.Email}' deleted by admin");
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

        [HttpGet]
        public async Task<IActionResult> ExportData()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                
                // Create CSV content
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("User ID,Full Name,Email,Phone,Created Date");

                foreach (var user in users)
                {
                    string fullname = user.FirstName +  " " + user.LastName;
                    csv.AppendLine($"{user.Id},{fullname},{user.Email},{user.PhoneNumber},{user.CreatedAt:yyyy-MM-dd}");
                }

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

        [HttpGet]
        public IActionResult UserGuide()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Security()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Reports()
        {
            return View();
        }
    }
}