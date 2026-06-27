using Microsoft.AspNetCore.Mvc;
using Sharply.Domain.Interfaces;
using Sharply.Web.ViewModels;

public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _authService.AuthenticateAsync(model.Email, model.Password);
        if (user == null)
        {
            ModelState.AddModelError("", "Correo o contraseña incorrectos.");
            return View(model);
        }
        return RedirectToAction("Index", "Home");
    }
}