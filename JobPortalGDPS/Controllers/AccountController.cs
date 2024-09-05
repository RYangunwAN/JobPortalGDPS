using Microsoft.AspNetCore.Mvc;
using JobPortalGDPS.Models;
using JobPortalGDPS.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace JobPortalGDPS.Controllers
{
    public class AccountController : Controller
    {
        private readonly DapperRepository _repository;

        // Constructor to inject DapperRepository
        public AccountController(DapperRepository repository)
        {
            _repository = repository;
        }

        // GET: Register
        // Displays the registration view
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register
        // Handles the registration form submission
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError(string.Empty, "Passwords do not match.");
                    return View(model);
                }

                var passwordHasher = new PasswordHasher<User>();
                var user = new User
                {
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber
                };

                user.PasswordHash = passwordHasher.HashPassword(user, model.Password);

                // Save user to database
                await _repository.AddUserAsync(user);

                return RedirectToAction("Login");
            }
            return View(model);
        }

        // GET: Login
        // Displays the login view
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        // Handles the login form submission
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _repository.GetUserByEmailAsync(model.Email);

                if (user != null)
                {
                    var passwordHasher = new PasswordHasher<User>();
                    var passwordResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

                    if (passwordResult == PasswordVerificationResult.Success)
                    {
                        // Set up authentication cookies
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),  // Include UserId in claims
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName)
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity));

                        return RedirectToAction("Index", "Home");
                    }
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        // POST: Logout
        // Handles user logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // GET: Profile
        // Displays the user's profile view
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                // Log the error or handle it accordingly
                return RedirectToAction("Error", "Home");
            }

            if (!int.TryParse(userIdString, out int userId))
            {
                // Handle error
                return RedirectToAction("Error", "Home");
            }

            var user = await _repository.GetUserByIdAsync(userId);
            if (user == null)
            {
                // Handle error
                return RedirectToAction("Error", "Home");
            }

            var model = new ProfileViewModel
            {
                UserId = user.UserId.ToString(),
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        // POST: Profile
        // Handles the profile update form submission
        [HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    ModelState.AddModelError(string.Empty, "Invalid user identifier.");
                    return View(model);
                }

                var user = await _repository.GetUserByIdAsync(userId);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                    return View(model);
                }

                // Update user properties
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;

                // Update user in the database
                await _repository.UpdateUserAsync(user);

                return RedirectToAction("Profile");
            }

            return View(model);
        }
    }
}
