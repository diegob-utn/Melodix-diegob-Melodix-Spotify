using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
      _logger.LogInformation("Attempting to register user with email: {Email}", model.Email);
      ViewData["ReturnUrl"] = returnUrl;

      if (!ModelState.IsValid)
      {
        _logger.LogWarning("Model state is invalid for user registration: {Email}", model.Email);

        // Log specific validation errors
        foreach (var modelError in ModelState)
        {
          var key = modelError.Key;
          var errors = modelError.Value.Errors;
          foreach (var error in errors)
          {
            _logger.LogError("Validation error for field {Field}: {Error}", key, error.ErrorMessage);
          }
        }

        return View(model);
      }

      // Check if Nick is already taken
      var existingUser = await _userManager.Users
        .FirstOrDefaultAsync(u => u.Nick == model.Nick);

      if (existingUser != null)
      {
        ModelState.AddModelError(nameof(model.Nick), "El nombre de usuario ya está en uso");
        return View(model);
      }

      var user = new ApplicationUser
      {
        UserName = model.Email,
        Email = model.Email,
        Nombre = model.Nombre,
        Nick = model.Nick,
        FechaNacimiento = model.FechaNacimiento,
        Genero = model.Genero,
        Rol = RolUsuario.Usuario,
        Activo = true,
        Verificado = false,
        CreadoEn = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
      };

      var result = await _userManager.CreateAsync(user, model.Password);

      if (result.Succeeded)
      {
        // Add user to the "Usuario" role
        await _userManager.AddToRoleAsync(user, "Usuario");

        _logger.LogInformation("Usuario {Email} creó una nueva cuenta con contraseña", model.Email);

        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToLocal(returnUrl);
      }

      foreach (var error in result.Errors)
      {
        _logger.LogError("Error creating user {Email}: {ErrorCode} - {ErrorDescription}",
          model.Email, error.Code, error.Description);
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
