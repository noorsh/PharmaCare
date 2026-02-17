using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaCare.Data.Models;
using PharmaCare.MVC.Models.ViewModels;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IPatientService _patientService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IPatientService patientService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _patientService = patientService;
            _logger = logger;
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
    }
}