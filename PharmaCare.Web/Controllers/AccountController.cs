using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaCare.Data.Models;
using PharmaCare.MVC.Models.ViewModels;
using PharmaCare.Services;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IPatientService _patientService;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IPatientService patientService,
            ILogger<AccountController> logger,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _patientService = patientService;
            _logger = logger;
            _emailService = emailService;
            _configuration = configuration;
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

      // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(PatientRegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Create user account
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign Patient role
                    await _userManager.AddToRoleAsync(user, "Patient");

                    // Ensure DateOfBirth is UTC
                    var dateOfBirth = model.DateOfBirth.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(model.DateOfBirth, DateTimeKind.Utc)
                        : model.DateOfBirth.ToUniversalTime();

                    // Create patient profile
                    var patient = new Patient
                    {
                        UserId = user.Id,
                        DateOfBirth = dateOfBirth,
                        Gender = model.Gender,
                        Address = model.Address,
                        City = model.City,
                        EmergencyContact = model.EmergencyContact,
                        Height = model.Height,
                        Weight = model.Weight,
                        BloodType = model.BloodType,
                        SmokingStatus = model.SmokingStatus,
                        AlcoholConsumption = model.AlcoholConsumption,
                        ExerciseFrequency = model.ExerciseFrequency,
                        IsPregnant = model.IsPregnant,
                        IsBreastfeeding = model.IsBreastfeeding,
                        // NEW: Critical Safety Flags
                        HasKidneyDisease = model.HasKidneyDisease,
                        HasLiverDisease = model.HasLiverDisease,
                        HasDrugAllergies = model.HasDrugAllergies,
                        DrugAllergyDetails = model.DrugAllergyDetails,
                        AdditionalNotes = model.AdditionalNotes,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _patientService.CreatePatientAsync(patient);
        //Send email 

                    var adminEmail = _configuration["EmailSettings:AdminEmail"];
                    var patientFullName = $"{model.FirstName} {model.LastName}";
                    // Send welcome email to patient
                    await _emailService.SendEmailAsync(
                        model.Email,
                        patientFullName,
                        "Welcome to PharmaCare 💊",
                        GetPatientWelcomeEmail(patientFullName)
                    );

                    // Send notification to admin
                    await _emailService.SendEmailAsync(
                        adminEmail!,
                        "PharmaCare Admin",
                        "New Patient Registration",
                        GetAdminNotificationEmail(patientFullName, model.Email)
                    );
                    // Sign in the user
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    TempData["Success"] = "Registration successful! Welcome to PharmaCare.";
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during patient registration");
                ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
            }

            return View(model);
        }
        // GET: Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {model.Email} logged in");
                    
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    if(User.IsInRole("Admin"))
                        return RedirectToAction("Dashboard", "Admin");
                    return RedirectToAction("Index", "Home");
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning($"User {model.Email} account locked out");
                    ModelState.AddModelError(string.Empty, "Account locked out. Please try again later.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
            }

            return View(model);
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out");
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            if (User.IsInRole("Patient"))
            {
                ViewBag.RedirectUrl = Url.Action("Dashboard", "Patient");
            }
            else if (User.IsInRole("Pharmacist"))
            {
                ViewBag.RedirectUrl = Url.Action("Index", "Pharmacist");
            }
            return View();
        }
        
        
        
        private string GetPatientWelcomeEmail(string patientName) => $"""
                                                                      <div style="font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;">
                                                                          <h2 style="color: #696cff;">Welcome to PharmaCare, {patientName}! 💊</h2>
                                                                          <p>Your account has been successfully created. You can now:</p>
                                                                          <ul>
                                                                              <li>Submit symptom consultations</li>
                                                                              <li>Receive pharmacist recommendations</li>
                                                                              <li>Track your consultation history</li>
                                                                          </ul>
                                                                          <a href="#" style="background:#696cff; color:white; padding:10px 20px; border-radius:5px; text-decoration:none;">
                                                                              Go to Dashboard
                                                                          </a>
                                                                          <p style="margin-top:20px; color:#888; font-size:12px;">PharmaCare — Your health, our priority.</p>
                                                                      </div>
                                                                      """;

        private string GetAdminNotificationEmail(string patientName, string patientEmail) => $"""
             <div style="font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;">
                 <h2 style="color: #696cff;">New Patient Registration 🔔</h2>
                 <p>A new patient has registered on PharmaCare:</p>
                 <table style="width:100%; border-collapse:collapse;">
                     <tr><td style="padding:8px; font-weight:bold;">Name:</td><td style="padding:8px;">{patientName}</td></tr>
                     <tr style="background:#f5f5f5;"><td style="padding:8px; font-weight:bold;">Email:</td><td style="padding:8px;">{patientEmail}</td></tr>
                     <tr><td style="padding:8px; font-weight:bold;">Date:</td><td style="padding:8px;">{DateTime.Now:MMMM dd, yyyy HH:mm}</td></tr>
                 </table>
                 <p style="margin-top:20px; color:#888; font-size:12px;">PharmaCare Admin Notification System</p>
             </div>
             """;
        // GET: Account/ChangePassword
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

// POST: Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Password updated successfully.";
                return RedirectToAction(nameof(ChangePassword));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
    }
    }
