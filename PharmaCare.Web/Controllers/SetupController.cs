using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaCare.Data.Models;

namespace PharmaCare.MVC.Controllers
{
    /// <summary>
    /// TEMPORARY CONTROLLER - Used only for initial setup
    /// </summary>
    [AllowAnonymous] 
    public class SetupController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<SetupController> _logger;

        public SetupController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<SetupController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new SetupViewModel
            {
                RolesExist = await CheckRolesExist(),
                AdminExists = await CheckAdminExists(),
                CurrentUserRoles = await GetCurrentUserRoles()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoles()
        {
            try
            {
                var roles = new[] { "Admin", "Pharmacist", "Patient" };

                foreach (var roleName in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        var role = new IdentityRole(roleName);
                        var result = await _roleManager.CreateAsync(role);

                        if (result.Succeeded)
                        {
                            _logger.LogInformation($"Role '{roleName}' created successfully");
                        }
                        else
                        {
                            _logger.LogError($"Failed to create role '{roleName}'");
                            TempData["Error"] = $"Failed to create role '{roleName}'";
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }

                TempData["Success"] = "All roles created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating roles");
                TempData["Error"] = "Error creating roles: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdminUser(string email, string password, string fullName)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fullName))
                {
                    TempData["Error"] = "All fields are required";
                    return RedirectToAction(nameof(Index));
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    TempData["Error"] = "User with this email already exists";
                    return RedirectToAction(nameof(Index));
                }

                // Create admin user
                var adminUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = fullName.Split(' ')[0],
                    LastName = fullName.Split(' ')[1],
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(adminUser, password);

                if (result.Succeeded)
                {
                    // Ensure Admin role exists
                    if (!await _roleManager.RoleExistsAsync("Admin"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    }

                    // Assign Admin role
                    var roleResult = await _userManager.AddToRoleAsync(adminUser, "Admin");

                    if (roleResult.Succeeded)
                    {
                        _logger.LogInformation($"Admin user '{email}' created successfully");
                        TempData["Success"] = $"Admin user '{email}' created successfully! You can now login.";
                    }
                    else
                    {
                        TempData["Error"] = "User created but failed to assign Admin role";
                        _logger.LogError($"Failed to assign Admin role to user '{email}'");
                    }
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    TempData["Error"] = "Failed to create user: " + errors;
                    _logger.LogError($"Failed to create admin user: {errors}");
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating admin user");
                TempData["Error"] = "Error creating admin user: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> MakeCurrentUserAdmin()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "You must be logged in to use this feature";
                    return RedirectToAction(nameof(Index));
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction(nameof(Index));
                }

                // Ensure Admin role exists
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // Check if already admin
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    TempData["Info"] = "You are already an Admin!";
                    return RedirectToAction(nameof(Index));
                }

                // Add Admin role
                var result = await _userManager.AddToRoleAsync(user, "Admin");

                if (result.Succeeded)
                {
                    // Sign out and sign back in to refresh claims
                    await _signInManager.SignOutAsync();
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    TempData["Success"] = "You are now an Admin! Roles refreshed.";
                    _logger.LogInformation($"User '{user.Email}' promoted to Admin");
                }
                else
                {
                    TempData["Error"] = "Failed to add Admin role";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making user admin");
                TempData["Error"] = "Error: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<bool> CheckRolesExist()
        {
            return await _roleManager.RoleExistsAsync("Admin") &&
                   await _roleManager.RoleExistsAsync("Pharmacist") &&
                   await _roleManager.RoleExistsAsync("Patient");
        }

        private async Task<bool> CheckAdminExists()
        {
            var users = _userManager.Users.ToList();
            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return true;
                }
            }
            return false;
        }

        private async Task<List<string>> GetCurrentUserRoles()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        return roles.ToList();
                    }
                }
            }
            return new List<string>();
        }
    }

    public class SetupViewModel
    {
        public bool RolesExist { get; set; }
        public bool AdminExists { get; set; }
        public List<string> CurrentUserRoles { get; set; } = new();
    }
}
