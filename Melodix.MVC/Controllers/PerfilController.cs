using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Melodix.Models;
using Melodix.Data;
using Melodix.Models.Models;
using Melodix.MVC.ViewModels;

namespace Melodix.MVC.Controllers
{
  /// <summary>
  /// Controlador para ver/editar perfil, mostrar estadísticas y listas públicas
  /// Modelos principales: ApplicationUser, HistorialEscucha, ListaReproduccion, UsuarioSigue
  /// </summary>
  [Authorize]
  public class PerfilController : Controller
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PerfilController> _logger;

    public PerfilController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ILogger<PerfilController> logger)
    {
      _userManager = userManager;
      _context = context;
      _logger = logger;
    }

    public async Task<IActionResult> Index(string? nick = null)
    {
      ApplicationUser? usuario;

      if (string.IsNullOrEmpty(nick))
      {
        // Ver propio perfil
        usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
          return RedirectToAction("Login", "Cuenta");
        }
      }
      else
      {
        // Ver perfil de otro usuario por nick
        usuario = await _context.Users
            .FirstOrDefaultAsync(u => u.Nick == nick);

        if (usuario == null)
        {
          return NotFound("Usuario no encontrado");
        }
      }

      var viewModel = new PerfilViewModel
      {
        Usuario = usuario,
        EsPropietario = User.Identity?.Name == usuario.Email,
        TotalEscuchas = await _context.HistorialesEscucha
              .CountAsync(h => h.UsuarioId == usuario.Id),
        TotalListas = await _context.ListasReproduccion
              .CountAsync(l => l.UsuarioId == usuario.Id && l.Publica),
        TotalSeguidores = await _context.UsuariosSigue
              .CountAsync(s => s.SeguidoId == usuario.Id),
        TotalSiguiendo = await _context.UsuariosSigue
              .CountAsync(s => s.SeguidorId == usuario.Id),
        ListasPublicas = await _context.ListasReproduccion
              .Where(l => l.UsuarioId == usuario.Id && l.Publica)
              .OrderByDescending(l => l.CreadoEn)
              .Take(6)
              .ToListAsync()
      };

      // Si no es el propietario, verificar si ya lo sigue
      if (!viewModel.EsPropietario)
      {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser != null)
        {
          viewModel.YaSigue = await _context.UsuariosSigue
              .AnyAsync(s => s.SeguidorId == currentUser.Id && s.SeguidoId == usuario.Id);
        }
      }

      return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return RedirectToAction("Login", "Cuenta");
      }

      var viewModel = new EditPerfilViewModel
      {
        Nombre = usuario.Nombre,
        Nick = usuario.Nick,
        Biografia = usuario.Biografia,
        Ubicacion = usuario.Ubicacion,
        FechaNacimiento = usuario.FechaNacimiento,
        Genero = usuario.Genero,
        FotoPerfil = usuario.FotoPerfil
      };

      return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditPerfilViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return RedirectToAction("Login", "Cuenta");
      }

      // Verificar si el nick ya está en uso por otro usuario
      if (model.Nick != usuario.Nick)
      {
        var nickExists = await _context.Users
            .AnyAsync(u => u.Nick == model.Nick && u.Id != usuario.Id);

        if (nickExists)
        {
          ModelState.AddModelError("Nick", "Este nick ya está en uso");
          return View(model);
        }
      }

      // Actualizar datos del usuario
      usuario.Nombre = model.Nombre;
      usuario.Nick = model.Nick;
      usuario.Biografia = model.Biografia;
      usuario.Ubicacion = model.Ubicacion;
      usuario.FechaNacimiento = model.FechaNacimiento;
      usuario.Genero = model.Genero;
      usuario.ActualizadoEn = DateTime.UtcNow;

      // Manejar subida de foto de perfil si se proporciona
      if (model.ArchivoFoto != null && model.ArchivoFoto.Length > 0)
      {
        var rutaFoto = await GuardarFotoPerfil(model.ArchivoFoto, usuario.Id);
        if (!string.IsNullOrEmpty(rutaFoto))
        {
          usuario.FotoPerfil = rutaFoto;
        }
      }

      var result = await _userManager.UpdateAsync(usuario);

      if (result.Succeeded)
      {
        TempData["Success"] = "Perfil actualizado correctamente";
        return RedirectToAction("Index");
      }

      foreach (var error in result.Errors)
      {
        ModelState.AddModelError(string.Empty, error.Description);
      }

      return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Seguir(string userId)
    {
      // TODO: Implementar sistema de seguimiento cuando se cree la entidad UsuariosSigue
      return Json(new { success = false, message = "Funcionalidad de seguimiento no implementada aún" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult NoSeguir(string userId)
    {
      // TODO: Implementar sistema de seguimiento cuando se cree la entidad UsuariosSigue
      return Json(new { success = false, message = "Funcionalidad de seguimiento no implementada aún" });
    }

    private async Task<string?> GuardarFotoPerfil(IFormFile archivo, string userId)
    {
      try
      {
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "subidos", "perfiles");
        Directory.CreateDirectory(uploadsFolder);

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var nombreArchivo = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        var rutaCompleta = Path.Combine(uploadsFolder, nombreArchivo);

        using (var stream = new FileStream(rutaCompleta, FileMode.Create))
        {
          await archivo.CopyToAsync(stream);
        }

        return $"/subidos/perfiles/{nombreArchivo}";
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al guardar foto de perfil para usuario {UserId}", userId);
        return null;
      }
    }
  }
}
