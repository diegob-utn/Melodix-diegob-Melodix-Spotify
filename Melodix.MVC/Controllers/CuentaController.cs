using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Melodix.Models;
using Melodix.Models.Models;
using Melodix.MVC.ViewModels;

namespace Melodix.MVC.Controllers
{
  /// <summary>
  /// Controlador para manejo de autenticación y registro de usuarios
  /// Registro, login, logout, recuperación de contraseña
  /// </summary>
  public class CuentaController : Controller
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<CuentaController> _logger;

    public CuentaController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<CuentaController> logger)
    {
      _userManager = userManager;
      _signInManager = signInManager;
      _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
      if (User.Identity?.IsAuthenticated == true)
      {
        return RedirectToAction("Index", "Inicio");
      }

      ViewData["ReturnUrl"] = returnUrl;
      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
      ViewData["ReturnUrl"] = returnUrl;

      if (!ModelState.IsValid)
      {
        return View(model);
      }

      var result = await _signInManager.PasswordSignInAsync(
          model.Email,
          model.Password,
          model.RememberMe,
          lockoutOnFailure: false);

      if (result.Succeeded)
      {
        _logger.LogInformation("Usuario {Email} inició sesión exitosamente", model.Email);
        return RedirectToLocal(returnUrl);
      }

      ModelState.AddModelError(string.Empty, "Intento de inicio de sesión inválido.");
      return View(model);
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
      if (User.Identity?.IsAuthenticated == true)
      {
        return RedirectToAction("Index", "Inicio");
      }

      ViewData["ReturnUrl"] = returnUrl;
      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
      ViewData["ReturnUrl"] = returnUrl;

      if (!ModelState.IsValid)
      {
        return View(model);
      }

      var user = new ApplicationUser
      {
        UserName = model.Email,
        Email = model.Email,
        Nombre = model.Nombre,
        Nick = model.Nick,
        Rol = RolUsuario.Usuario,
        Activo = true,
        Verificado = false,
        CreadoEn = DateTime.UtcNow
      };

      var result = await _userManager.CreateAsync(user, model.Password);

      if (result.Succeeded)
      {
        _logger.LogInformation("Usuario {Email} creó una nueva cuenta con contraseña", model.Email);

        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToLocal(returnUrl);
      }

      foreach (var error in result.Errors)
      {
        ModelState.AddModelError(string.Empty, error.Description);
      }

      return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
      await _signInManager.SignOutAsync();
      _logger.LogInformation("Usuario cerró sesión");
      return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
      return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
      if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
      {
        return Redirect(returnUrl);
      }

      return RedirectToAction("Index", "Inicio");
    }
  }
}
