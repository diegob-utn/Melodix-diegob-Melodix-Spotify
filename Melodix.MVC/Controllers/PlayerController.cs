using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Melodix.Data;
using Melodix.Models;
using Melodix.Models.Models;
using Melodix.MVC.ViewModels;

namespace Melodix.MVC.Controllers
{
  /// <summary>
  /// Controlador para funcionalidades de reproducción y subida de música
  /// </summary>
  [AllowAnonymous]
  public class PlayerController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<PlayerController> _logger;

    public PlayerController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment environment,
        ILogger<PlayerController> logger)
    {
      _context = context;
      _userManager = userManager;
      _environment = environment;
      _logger = logger;
    }

    #region Subida de Música

    /// <summary>
    /// Página principal del reproductor
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
      return View();
    }

    /// <summary>
    /// Mostrar formulario para subir pista
    /// </summary>
    [HttpGet]
    public IActionResult SubirPista()
    {
      var viewModel = new SubirPistaViewModel();
      return View(viewModel);
    }

    /// <summary>
    /// Procesar subida de pista
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubirPista(SubirPistaViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      try
      {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
          TempData["Error"] = "Debes iniciar sesión para subir música";
          return View(model);
        }

        // Procesar archivo de audio
        string? rutaArchivo = null;
        if (model.ArchivoAudio != null && model.ArchivoAudio.Length > 0)
        {
          rutaArchivo = await GuardarArchivoAudio(model.ArchivoAudio);
          if (rutaArchivo == null)
          {
            ModelState.AddModelError("ArchivoAudio", "Error al procesar el archivo de audio");
            return View(model);
          }
        }

        // Crear nueva pista
        var nuevaPista = new Pista
        {
          Titulo = model.Titulo,
          Genero = model.Genero,
          FechaLanzamiento = DateTime.Now,
          EsExplicita = model.EsExplicita,
          RutaArchivo = rutaArchivo,
          UsuarioId = usuario.Id,
          AlbumId = model.AlbumId ?? 0, // Usar 0 como valor por defecto si es null
          CreadoEn = DateTime.UtcNow,
          ActualizadoEn = DateTime.UtcNow,
          FechaSubida = DateTime.UtcNow,
          ContadorReproducciones = 0
        };

        _context.Pistas.Add(nuevaPista);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Pista '{model.Titulo}' subida exitosamente";
        return RedirectToAction("MisPistas", "Musica");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al subir pista");
        ModelState.AddModelError("", "Ocurrió un error al subir la pista");
        return View(model);
      }
    }

    #endregion

    #region Reproducción

    /// <summary>
    /// Obtener datos de una pista para reproducción
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerPista(int id)
    {
      try
      {
        var pista = await _context.Pistas
            .Include(p => p.Album)
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pista == null)
        {
          return NotFound(new { success = false, message = "Pista no encontrada" });
        }

        var pistaDatos = new
        {
          id = pista.Id,
          titulo = pista.Titulo,
          artista = pista.Usuario?.UserName ?? "Artista Desconocido",
          album = pista.Album?.Titulo ?? "Sencillo",
          rutaArchivo = pista.RutaArchivo,
          genero = pista.Genero.ToString(),
          esExplicita = pista.EsExplicita
        };

        return Json(new { success = true, pista = pistaDatos });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al obtener datos de pista {PistaId}", id);
        return Json(new { success = false, message = "Error al cargar la pista" });
      }
    }

    /// <summary>
    /// Obtener playlist de reproducción
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerPlaylist(int? albumId = null, string? genero = null, string? artistaId = null)
    {
      try
      {
        var query = _context.Pistas
            .Include(p => p.Album)
            .Include(p => p.Usuario)
            .Where(p => !string.IsNullOrEmpty(p.RutaArchivo));

        // Filtros opcionales
        if (albumId.HasValue && albumId.Value > 0)
          query = query.Where(p => p.AlbumId == albumId.Value);

        if (!string.IsNullOrEmpty(genero) && Enum.TryParse<GeneroMusica>(genero, out var generoEnum))
          query = query.Where(p => p.Genero == generoEnum);

        if (!string.IsNullOrEmpty(artistaId))
          query = query.Where(p => p.UsuarioId == artistaId);

        var pistas = await query
            .OrderBy(p => p.Titulo)
            .Select(p => new
            {
              id = p.Id,
              titulo = p.Titulo,
              artista = p.Usuario!.UserName ?? "Artista Desconocido",
              album = p.Album != null ? p.Album.Titulo : "Sencillo",
              rutaArchivo = p.RutaArchivo,
              genero = p.Genero.ToString(),
              esExplicita = p.EsExplicita
            })
            .ToListAsync();

        return Json(new { success = true, pistas });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al obtener playlist");
        return Json(new { success = false, message = "Error al cargar la playlist" });
      }
    }

    /// <summary>
    /// Registrar reproducción de una pista
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RegistrarReproduccion(int pistaId)
    {
      try
      {
        var usuario = await _userManager.GetUserAsync(User);
        var pista = await _context.Pistas.FindAsync(pistaId);

        if (pista == null)
        {
          return Json(new { success = false, message = "Pista no encontrada" });
        }

        // Registrar en historial si hay usuario autenticado
        if (usuario != null)
        {
          var historial = new HistorialEscucha
          {
            UsuarioId = usuario.Id,
            PistaId = pistaId,
            EscuchadaEn = DateTime.UtcNow
          };

          _context.HistorialesEscucha.Add(historial);
        }

        // Incrementar contador de reproducciones de la pista
        pista.ContadorReproducciones++;
        pista.ActualizadoEn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Json(new { success = true });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al registrar reproducción de pista {PistaId}", pistaId);
        return Json(new { success = false, message = "Error al registrar reproducción" });
      }
    }

    #endregion

    #region Métodos Auxiliares

    /// <summary>
    /// Guardar archivo de audio en el servidor
    /// </summary>
    private async Task<string?> GuardarArchivoAudio(IFormFile archivo)
    {
      try
      {
        // Validar tipo de archivo
        var extensionesPermitidas = new[] { ".mp3", ".wav", ".flac", ".aac", ".ogg" };
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

        if (!extensionesPermitidas.Contains(extension))
        {
          return null;
        }

        // Crear directorio si no existe
        var directorioAudio = Path.Combine(_environment.WebRootPath, "uploads", "audio");
        if (!Directory.Exists(directorioAudio))
        {
          Directory.CreateDirectory(directorioAudio);
        }

        // Generar nombre único
        var nombreArchivo = $"{Guid.NewGuid()}{extension}";
        var rutaCompleta = Path.Combine(directorioAudio, nombreArchivo);

        // Guardar archivo
        using (var stream = new FileStream(rutaCompleta, FileMode.Create))
        {
          await archivo.CopyToAsync(stream);
        }

        return $"/uploads/audio/{nombreArchivo}";
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al guardar archivo de audio");
        return null;
      }
    }

    #endregion
  }
}