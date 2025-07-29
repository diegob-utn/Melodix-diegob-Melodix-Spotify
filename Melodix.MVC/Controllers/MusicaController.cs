using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Melodix.Models.Models;
using Melodix.MVC.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Melodix.Data;
using Melodix.Models;

namespace Melodix.MVC.Controllers
{
  [AllowAnonymous]
  public class MusicaController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<MusicaController> _logger;

    public MusicaController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment environment,
        ILogger<MusicaController> logger)
    {
      _context = context;
      _userManager = userManager;
      _environment = environment;
      _logger = logger;
    }

    // GET: Musica
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
        return RedirectToAction("Login", "Account");

      var albums = await _context.Albums
          .Where(a => a.UsuarioId == usuario.Id)
          .Include(a => a.Pistas)
          .OrderByDescending(a => a.FechaLanzamiento)
          .ToListAsync();

      return View(albums);
    }

    // GET: Musica/MisAlbums
    [AllowAnonymous]
    public async Task<IActionResult> MisAlbums()
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
        return RedirectToAction("Login", "Account");

      var albums = await _context.Albums
          .Where(a => a.UsuarioId == usuario.Id)
          .Include(a => a.Pistas)
          .OrderByDescending(a => a.FechaLanzamiento)
          .ToListAsync();

      return View(albums);
    }

    // GET: Musica/CrearAlbum
    [AllowAnonymous]
    public IActionResult CrearAlbum()
    {
      return View(new CrearAlbumViewModel());
    }

    // POST: Musica/CrearAlbum
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> CrearAlbum(CrearAlbumViewModel model)
    {
      if (!ModelState.IsValid)
        return View(model);

      try
      {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
          return RedirectToAction("Login", "Account");

        var album = new Album
        {
          Titulo = model.Titulo,
          Descripcion = model.Descripcion,
          FechaLanzamiento = model.FechaLanzamiento,
          UsuarioId = usuario.Id
        };

        // Crear directorio para álbumes si no existe
        var rutaAlbums = Path.Combine(_environment.WebRootPath, "uploads", "albums");
        if (!Directory.Exists(rutaAlbums))
        {
          Directory.CreateDirectory(rutaAlbums);
        }

        _context.Albums.Add(album);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Álbum creado exitosamente";
        return RedirectToAction(nameof(MisAlbums));
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al crear álbum");
        ModelState.AddModelError("", "Ocurrió un error al crear el álbum");
        return View(model);
      }
    }

    // GET: Musica/SubirPista
    [AllowAnonymous]
    public async Task<IActionResult> SubirPista()
    {
      try
      {
        var usuario = await _userManager.GetUserAsync(User);
        
        var model = new SubirPistaViewModel();

        // Cargar álbumes del usuario actual o lista vacía si no hay usuario
        if (usuario != null)
        {
          ViewBag.Albums = await _context.Albums
              .Where(a => a.UsuarioId == usuario.Id)
              .Select(a => new { a.Id, a.Titulo })
              .ToListAsync();
        }
        else
        {
          ViewBag.Albums = new List<object>();
        }

        // Cargar géneros desde enum
        ViewBag.Generos = Enum.GetValues<GeneroMusica>()
            .Select(g => new { Id = (int)g, Nombre = g.ToString() })
            .ToList();

        return View(model);
      }
      catch (Exception ex)
      {
        return Content($"<h1>Error en SubirPista</h1><p>{ex.Message}</p><p><a href='/'>Volver al inicio</a></p>", "text/html");
      }
    }

    // GET: Musica/Test - Acción de prueba temporal
    [AllowAnonymous]
    public IActionResult Test()
    {
      return Content("¡Funciona! Esta es una página de prueba sin autorización. El controlador MusicaController está funcionando correctamente.", "text/html");
    }

    // GET: Musica/SubirPistaDirecto - Bypass completo de autorización
    [AllowAnonymous]
    public IActionResult SubirPistaDirecto()
    {
      try 
      {
        var model = new SubirPistaViewModel();
        
        // Datos hardcodeados para testing
        ViewBag.Albums = new List<object>();
        ViewBag.Generos = Enum.GetValues<GeneroMusica>()
            .Select(g => new { Id = (int)g, Nombre = g.ToString() })
            .ToList();

        return View("SubirPista", model);
      }
      catch (Exception ex)
      {
        return Content($"Error: {ex.Message}<br>StackTrace: {ex.StackTrace}", "text/html");
      }
    }

    // GET: Musica/CambiarRol - Acción temporal para cambiar rol a Musico
    [AllowAnonymous]
    public async Task<IActionResult> CambiarRol()
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return Content("No hay usuario autenticado. Por favor, inicia sesión primero.<br><a href='/Cuenta/Login'>Iniciar Sesión</a>", "text/html");
      }

      // Obtener roles actuales
      var rolesActuales = await _userManager.GetRolesAsync(usuario);
      
      // Remover rol Usuario si existe
      if (rolesActuales.Contains("Usuario"))
      {
        await _userManager.RemoveFromRoleAsync(usuario, "Usuario");
      }

      // Agregar rol Musico si no lo tiene
      if (!rolesActuales.Contains("Musico"))
      {
        var resultado = await _userManager.AddToRoleAsync(usuario, "Musico");
        if (!resultado.Succeeded)
        {
          var errores = string.Join(", ", resultado.Errors.Select(e => e.Description));
          return Content($"Error al asignar rol: {errores}", "text/html");
        }
      }

      // Verificar roles después del cambio
      var nuevosRoles = await _userManager.GetRolesAsync(usuario);
      
      return Content($"<h2>Información de Usuario y Roles</h2>" +
                    $"<p><strong>Usuario:</strong> {usuario.Email}</p>" +
                    $"<p><strong>Roles actuales:</strong> {string.Join(", ", nuevosRoles)}</p>" +
                    $"<br><a href='/Musica/SubirPista' style='padding: 10px; background: #007bff; color: white; text-decoration: none; border-radius: 5px;'>Ir a Subir Pista</a>", "text/html");
    }

    // GET: Musica/VerificarAcceso - Verificar el acceso actual del usuario
    [AllowAnonymous]
    public async Task<IActionResult> VerificarAcceso()
    {
      var usuario = await _userManager.GetUserAsync(User);
      var roles = usuario != null ? await _userManager.GetRolesAsync(usuario) : new List<string>();
      
      return Content($"<h2>Verificación de Acceso</h2>" +
                    $"<p><strong>Usuario autenticado:</strong> {usuario?.Email ?? "No autenticado"}</p>" +
                    $"<p><strong>Roles:</strong> {string.Join(", ", roles)}</p>" +
                    $"<br><a href='/Musica/SubirPista'>Probar SubirPista</a>", "text/html");
    }

    // POST: Musica/SubirPista
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> SubirPista(SubirPistaViewModel model)
    {
      if (!ModelState.IsValid)
      {
        // Recargar listas para la vista
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario != null)
        {
          ViewBag.Albums = await _context.Albums
              .Where(a => a.UsuarioId == usuario.Id)
              .Select(a => new { a.Id, a.Titulo })
              .ToListAsync();
        }
        else
        {
          ViewBag.Albums = new List<object>();
        }

        ViewBag.Generos = Enum.GetValues<GeneroMusica>()
            .Select(g => new { Id = (int)g, Nombre = g.ToString() })
            .ToList();

        return View(model);
      }

      try
      {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
          return RedirectToAction("Login", "Account");

        var pista = new Pista
        {
          Titulo = model.Titulo,
          AlbumId = model.AlbumId ?? 0,
          Genero = model.Genero,
          EsExplicita = model.EsExplicita,
          FechaLanzamiento = DateTime.UtcNow,
          UsuarioId = usuario.Id
        };

        // Crear directorios si no existen
        var rutaAudio = Path.Combine(_environment.WebRootPath, "uploads", "audio");
        if (!Directory.Exists(rutaAudio))
          Directory.CreateDirectory(rutaAudio);

        // Procesar archivo de audio
        if (model.ArchivoAudio != null && model.ArchivoAudio.Length > 0)
        {
          var extensionesAudio = new[] { ".mp3", ".wav", ".m4a", ".flac" };
          var extensionAudio = Path.GetExtension(model.ArchivoAudio.FileName).ToLowerInvariant();

          if (!extensionesAudio.Contains(extensionAudio))
          {
            ModelState.AddModelError("ArchivoAudio", "Solo se permiten archivos MP3, WAV, M4A y FLAC");
            await RecargarListasSubirPista();
            return View(model);
          }

          var nombreAudio = $"{Guid.NewGuid()}{extensionAudio}";
          var rutaCompletaAudio = Path.Combine(rutaAudio, nombreAudio);

          using (var stream = new FileStream(rutaCompletaAudio, FileMode.Create))
          {
            await model.ArchivoAudio.CopyToAsync(stream);
          }

          pista.RutaArchivo = $"/uploads/audio/{nombreAudio}";
          pista.UrlArchivo = pista.RutaArchivo;
        }

        _context.Pistas.Add(pista);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Pista subida exitosamente";
        return RedirectToAction(nameof(MisPistas));
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al subir pista");
        ModelState.AddModelError("", "Ocurrió un error al subir la pista");
        await RecargarListasSubirPista();
        return View(model);
      }
    }

    // GET: Musica/MisPistas
    [AllowAnonymous]
    public async Task<IActionResult> MisPistas()
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
        return RedirectToAction("Login", "Account");

      var pistas = await _context.Pistas
          .Where(p => p.UsuarioId == usuario.Id)
          .Include(p => p.Album)
          .OrderByDescending(p => p.FechaLanzamiento)
          .ToListAsync();

      var model = new MisPistasViewModel
      {
        Pistas = pistas,
        PaginaActual = 1,
        TotalPaginas = 1,
        TotalResultados = pistas.Count
      };

      return View(model);
    }

    // GET: Musica/EditarPista/5
    [AllowAnonymous]
    public async Task<IActionResult> EditarPista(int id)
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
        return RedirectToAction("Login", "Account");

      var pista = await _context.Pistas
          .Include(p => p.Album)
          .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuario.Id);

      if (pista == null)
        return NotFound();

      var model = new EditarPistaViewModel
      {
        Id = pista.Id,
        Titulo = pista.Titulo,
        AlbumId = pista.AlbumId,
        Genero = pista.Genero,
        EsExplicita = pista.EsExplicita
      };

      // Cargar listas para dropdowns
      ViewBag.Albums = await _context.Albums
          .Where(a => a.UsuarioId == usuario.Id)
          .Select(a => new { a.Id, a.Titulo })
          .ToListAsync();

      ViewBag.Generos = Enum.GetValues<GeneroMusica>()
          .Select(g => new { Id = (int)g, Nombre = g.ToString() })
          .ToList();

      return View(model);
    }

    // POST: Musica/EditarPista/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> EditarPista(int id, EditarPistaViewModel model)
    {
      if (id != model.Id)
        return NotFound();

      if (!ModelState.IsValid)
      {
        await RecargarListasEditarPista();
        return View(model);
      }

      try
      {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
          return RedirectToAction("Login", "Account");

        var pista = await _context.Pistas
            .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuario.Id);

        if (pista == null)
          return NotFound();

        // Actualizar propiedades básicas
        pista.Titulo = model.Titulo;
        pista.AlbumId = model.AlbumId ?? 0;
        pista.Genero = model.Genero;
        pista.EsExplicita = model.EsExplicita;

        _context.Update(pista);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Pista actualizada exitosamente";
        return RedirectToAction(nameof(MisPistas));
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al editar pista");
        ModelState.AddModelError("", "Ocurrió un error al editar la pista");
        await RecargarListasEditarPista();
        return View(model);
      }
    }

    // POST: Musica/EliminarPista/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> EliminarPista(int id)
    {
      try
      {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
          return RedirectToAction("Login", "Account");

        var pista = await _context.Pistas
            .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuario.Id);

        if (pista == null)
          return NotFound();

        // Eliminar archivos físicos
        if (!string.IsNullOrEmpty(pista.RutaArchivo))
        {
          var rutaArchivo = Path.Combine(_environment.WebRootPath, pista.RutaArchivo.TrimStart('/'));
          if (System.IO.File.Exists(rutaArchivo))
          {
            System.IO.File.Delete(rutaArchivo);
          }
        }

        _context.Pistas.Remove(pista);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Pista eliminada exitosamente";
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al eliminar pista");
        TempData["Error"] = "Ocurrió un error al eliminar la pista";
      }

      return RedirectToAction(nameof(MisPistas));
    }

    // POST: Musica/EliminarAlbum/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> EliminarAlbum(int id)
    {
      try
      {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
          return RedirectToAction("Login", "Account");

        var album = await _context.Albums
            .Include(a => a.Pistas)
            .FirstOrDefaultAsync(a => a.Id == id && a.UsuarioId == usuario.Id);

        if (album == null)
          return NotFound();

        // Eliminar archivos de las pistas del álbum
        foreach (var pista in album.Pistas)
        {
          if (!string.IsNullOrEmpty(pista.RutaArchivo))
          {
            var rutaArchivo = Path.Combine(_environment.WebRootPath, pista.RutaArchivo.TrimStart('/'));
            if (System.IO.File.Exists(rutaArchivo))
            {
              System.IO.File.Delete(rutaArchivo);
            }
          }
        }

        _context.Albums.Remove(album);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Álbum eliminado exitosamente";
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al eliminar álbum");
        TempData["Error"] = "Ocurrió un error al eliminar el álbum";
      }

      return RedirectToAction(nameof(MisAlbums));
    }

    // Métodos auxiliares privados
    private async Task RecargarListasSubirPista()
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario != null)
      {
        ViewBag.Albums = await _context.Albums
            .Where(a => a.UsuarioId == usuario.Id)
            .Select(a => new { a.Id, a.Titulo })
            .ToListAsync();
      }
      else
      {
        ViewBag.Albums = new List<object>();
      }

      ViewBag.Generos = Enum.GetValues<GeneroMusica>()
          .Select(g => new { Id = (int)g, Nombre = g.ToString() })
          .ToList();
    }

    private async Task RecargarListasEditarPista()
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario != null)
      {
        ViewBag.Albums = await _context.Albums
            .Where(a => a.UsuarioId == usuario.Id)
            .Select(a => new { a.Id, a.Titulo })
            .ToListAsync();
      }
      else
      {
        ViewBag.Albums = new List<object>();
      }

      ViewBag.Generos = Enum.GetValues<GeneroMusica>()
          .Select(g => new { Id = (int)g, Nombre = g.ToString() })
          .ToList();
    }
  }
}
