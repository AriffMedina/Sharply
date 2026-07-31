using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;
using Sharply.Web.ViewModels;
using System.Security.Claims;

namespace Sharply.Web.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly IUserRepository _userRepository;

        public SettingsController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userRepository.GetByIdAsync(CurrentUserId);
            if (user is null) return NotFound();

            ViewData["Title"] = "Settings";
            return View(new SettingsViewModel
            {
                Name = user.Name,
                Email = user.Email,
                DecayEmailEnabled = user.DecayEmailEnabled,
                DecayRetentionThreshold = user.DecayRetentionThreshold,
                DecayStrategy = user.DecayStrategy
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Settings";
                return View(model);
            }

            var user = await _userRepository.GetByIdAsync(CurrentUserId);
            if (user is null) return NotFound();

            user.Name = model.Name.Trim();
            user.Email = model.Email.Trim();
            user.DecayEmailEnabled = model.DecayEmailEnabled;
            user.DecayRetentionThreshold = model.DecayRetentionThreshold;
            user.DecayStrategy = model.DecayStrategy;

            await _userRepository.UpdateAsync(user);

            // El nombre/email viven como claims en la cookie de sesión: hay que
            // reemitirla para que el cambio se refleje sin pedir un nuevo login.
            await SignInUserAsync(user);

            TempData["SettingsSaved"] = true;
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            await _userRepository.DeleteAsync(CurrentUserId);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        private async Task SignInUserAsync(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Email, user.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true });
        }
    }
}
