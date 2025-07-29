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
  /// Controlador para el reproductor de música integrado
  /// Maneja tanto la subida como la reproducción de archivos de audio
  /// </summary>
  [AllowAnonymous]
  public class MusicPlayerController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<MusicPlayerController> _logger;

    public MusicPlayerController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment environment,
        ILogger<MusicPlayerController> logger)
    {
      _context = context;
      _userManager = userManager;
      _environment = environment;
      _logger = logger;
    }

    #region Página Principal del Reproductor

    /// <summary>
    /// Página principal del reproductor con todas las funcionalidades
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
      var viewModel = new MusicPlayerViewModel
      {
        Pistas = await ObtenerTodasLasPistas(),
        Generos = Enum.GetValues<GeneroMusica>().ToList(),
        PuedeSubir = User.Identity?.IsAuthenticated ?? false
      };

      return View(viewModel);
    }

    #endregion

    #region Subida de Música

    /// <summary>
    /// Subir nueva pista de música
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubirPista(IFormFile archivo, string titulo, GeneroMusica genero, bool esExplicita)
    {
      try
      {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
          return Json(new { success = false, message = "Debes iniciar sesión para subir música" });
        }

        if (archivo == null || archivo.Length == 0)
        {
          return Json(new { success = false, message = "Selecciona un archivo de audio" });
        }

        if (string.IsNullOrWhiteSpace(titulo))
        {
          return Json(new { success = false, message = "El título es requerido" });
        }

        // Validar archivo de audio
        var extensionesPermitidas = new[] { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".m4a" };
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

        if (!extensionesPermitidas.Contains(extension))
        {
          return Json(new { success = false, message = "Formato de archivo no válido. Usa: MP3, WAV, FLAC, AAC, OGG, M4A" });
        }

        // Validar tamaño (máximo 100MB)
        if (archivo.Length > 100 * 1024 * 1024)
        {
          return Json(new { success = false, message = "El archivo es demasiado grande. Máximo 100MB" });
        }

        // Guardar archivo
        var rutaArchivo = await GuardarArchivoAudio(archivo);
        if (rutaArchivo == null)
        {
          return Json(new { success = false, message = "Error al guardar el archivo" });
        }

        // Obtener o crear álbum por defecto para singles
        var albumDefecto = await ObtenerOCrearAlbumDefecto(usuario);

        // Crear nueva pista
        var nuevaPista = new Pista
        {
          Titulo = titulo.Trim(),
          Genero = genero,
          EsExplicita = esExplicita,
          RutaArchivo = rutaArchivo,
          UsuarioId = usuario.Id,
          AlbumId = albumDefecto.Id,
          FechaLanzamiento = DateTime.Now,
          FechaSubida = DateTime.UtcNow,
          CreadoEn = DateTime.UtcNow,
          ActualizadoEn = DateTime.UtcNow,
          ContadorReproducciones = 0
        };

        _context.Pistas.Add(nuevaPista);
        await _context.SaveChangesAsync();

        // Devolver datos de la pista para agregarla inmediatamente a la playlist
        var pistaDatos = new
        {
          id = nuevaPista.Id,
          titulo = nuevaPista.Titulo,
          artista = usuario.UserName ?? "Usuario",
          genero = nuevaPista.Genero.ToString(),
          rutaArchivo = nuevaPista.RutaArchivo,
          esExplicita = nuevaPista.EsExplicita,
          fechaSubida = nuevaPista.FechaSubida.ToString("dd/MM/yyyy HH:mm")
        };

        return Json(new { success = true, message = $"Pista '{titulo}' subida exitosamente", pista = pistaDatos });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al subir pista");
        return Json(new { success = false, message = "Error interno del servidor" });
      }
    }

    #endregion

    #region API para Reproductor

    /// <summary>
    /// Obtener todas las pistas disponibles
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerPistas(GeneroMusica? genero = null, string? busqueda = null)
    {
      try
      {
        var query = _context.Pistas
            .Include(p => p.Usuario)
            .Include(p => p.Album)
            .Where(p => !string.IsNullOrEmpty(p.RutaArchivo));

        // Filtrar por género
        if (genero.HasValue && genero.Value != GeneroMusica.Desconocido)
        {
          query = query.Where(p => p.Genero == genero.Value);
        }

        // Filtrar por búsqueda
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
          busqueda = busqueda.ToLower();
          query = query.Where(p =>
              p.Titulo.ToLower().Contains(busqueda) ||
              p.Usuario.UserName!.ToLower().Contains(busqueda) ||
              (p.Album != null && p.Album.Titulo.ToLower().Contains(busqueda))
          );
        }

        var pistas = await query
            .OrderByDescending(p => p.FechaSubida)
            .Select(p => new
            {
              id = p.Id,
              titulo = p.Titulo,
              artista = p.Usuario.UserName ?? "Usuario Desconocido",
              album = p.Album != null ? p.Album.Titulo : "Sencillo",
              genero = p.Genero.ToString(),
              rutaArchivo = p.RutaArchivo,
              esExplicita = p.EsExplicita,
              reproducciones = p.ContadorReproducciones,
              fechaSubida = p.FechaSubida.ToString("dd/MM/yyyy")
            })
            .ToListAsync();

        return Json(new { success = true, pistas });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al obtener pistas");
        return Json(new { success = false, message = "Error al cargar las pistas" });
      }
    }

    /// <summary>
    /// Registrar reproducción de una pista
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ReproducirPista(int pistaId)
    {
      try
      {
        var pista = await _context.Pistas.FindAsync(pistaId);
        if (pista == null)
        {
          return Json(new { success = false, message = "Pista no encontrada" });
        }

        // Incrementar contador
        pista.ContadorReproducciones++;
        pista.ActualizadoEn = DateTime.UtcNow;

        // Registrar en historial si hay usuario autenticado
        var usuario = await _userManager.GetUserAsync(User);
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

        await _context.SaveChangesAsync();

        return Json(new { success = true, reproducciones = pista.ContadorReproducciones });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al registrar reproducción de pista {PistaId}", pistaId);
        return Json(new { success = false, message = "Error al registrar reproducción" });
      }
    }

    /// <summary>
    /// Obtener información detallada de una pista
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> InfoPista(int id)
    {
      try
      {
        var pista = await _context.Pistas
            .Include(p => p.Usuario)
            .Include(p => p.Album)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pista == null)
        {
          return Json(new { success = false, message = "Pista no encontrada" });
        }

        var info = new
        {
          id = pista.Id,
          titulo = pista.Titulo,
          artista = pista.Usuario?.UserName ?? "Usuario Desconocido",
          album = pista.Album?.Titulo ?? "Sencillo",
          genero = pista.Genero.ToString(),
          esExplicita = pista.EsExplicita,
          reproducciones = pista.ContadorReproducciones,
          fechaLanzamiento = pista.FechaLanzamiento?.ToString("dd/MM/yyyy"),
          fechaSubida = pista.FechaSubida.ToString("dd/MM/yyyy HH:mm")
        };

        return Json(new { success = true, info });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al obtener información de pista {PistaId}", id);
        return Json(new { success = false, message = "Error al obtener información" });
      }
    }

    /// <summary>
    /// Eliminar pista (solo el propietario)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarPista(int pistaId)
    {
      try
      {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
          return Json(new { success = false, message = "Debes iniciar sesión" });
        }

        var pista = await _context.Pistas
            .FirstOrDefaultAsync(p => p.Id == pistaId && p.UsuarioId == usuario.Id);

        if (pista == null)
        {
          return Json(new { success = false, message = "Pista no encontrada o sin permisos" });
        }

        // Eliminar archivo físico
        if (!string.IsNullOrEmpty(pista.RutaArchivo))
        {
          var rutaCompleta = Path.Combine(_environment.WebRootPath, pista.RutaArchivo.TrimStart('/'));
          if (System.IO.File.Exists(rutaCompleta))
          {
            System.IO.File.Delete(rutaCompleta);
          }
        }

        _context.Pistas.Remove(pista);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = $"Pista '{pista.Titulo}' eliminada exitosamente" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al eliminar pista {PistaId}", pistaId);
        return Json(new { success = false, message = "Error al eliminar la pista" });
      }
    }

    #endregion

    #region Métodos Auxiliares

    /// <summary>
    /// Obtener todas las pistas para la vista inicial
    /// </summary>
    private async Task<List<PistaMusicPlayerViewModel>> ObtenerTodasLasPistas()
    {
      try
      {
        var userId = _userManager.GetUserId(User);

        return await _context.Pistas
            .Include(p => p.Usuario)
            .Include(p => p.Album)
            .Where(p => !string.IsNullOrEmpty(p.RutaArchivo))
            .OrderByDescending(p => p.FechaSubida)
            .Select(p => new PistaMusicPlayerViewModel
            {
              Id = p.Id,
              Titulo = p.Titulo,
              Artista = p.Usuario.UserName ?? "Usuario Desconocido",
              Album = p.Album != null ? p.Album.Titulo : "Sencillo",
              Genero = p.Genero,
              RutaArchivo = p.RutaArchivo ?? string.Empty,
              EsExplicita = p.EsExplicita,
              Reproducciones = p.ContadorReproducciones,
              FechaSubida = p.FechaSubida,
              EsMia = p.UsuarioId == userId ? true : false
            })
            .ToListAsync();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al obtener pistas");
        return new List<PistaMusicPlayerViewModel>();
      }
    }

    /// <summary>
    /// Guardar archivo de audio en el servidor
    /// </summary>
    private async Task<string?> GuardarArchivoAudio(IFormFile archivo)
    {
      try
      {
        // Crear directorio si no existe
        var directorioAudio = Path.Combine(_environment.WebRootPath, "uploads", "music");
        if (!Directory.Exists(directorioAudio))
        {
          Directory.CreateDirectory(directorioAudio);
        }

        // Generar nombre único preservando la extensión
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var nombreArchivo = $"{Guid.NewGuid()}{extension}";
        var rutaCompleta = Path.Combine(directorioAudio, nombreArchivo);

        // Guardar archivo
        using (var stream = new FileStream(rutaCompleta, FileMode.Create))
        {
          await archivo.CopyToAsync(stream);
        }

        return $"/uploads/music/{nombreArchivo}";
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al guardar archivo de audio");
        return null;
      }
    }

    /// <summary>
    /// Obtener o crear álbum por defecto para singles del usuario
    /// </summary>
    private async Task<Album> ObtenerOCrearAlbumDefecto(ApplicationUser usuario)
    {
      try
      {
        // Buscar si ya existe un álbum "Singles" para este usuario
        var albumExistente = await _context.Albums
            .FirstOrDefaultAsync(a => a.UsuarioId == usuario.Id && a.Titulo == "Singles");

        if (albumExistente != null)
        {
          return albumExistente;
        }

        // Crear nuevo álbum por defecto
        var nuevoAlbum = new Album
        {
          Titulo = "Singles",
          Descripcion = "Álbum automático para pistas individuales",
          FechaLanzamiento = DateTime.Now,
          UsuarioId = usuario.Id
        };

        _context.Albums.Add(nuevoAlbum);
        await _context.SaveChangesAsync();

        return nuevoAlbum;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al obtener o crear álbum por defecto para usuario {UserId}", usuario.Id);
        throw; // Re-throw para que el método padre maneje el error
      }
    }

    #endregion
  }


}
