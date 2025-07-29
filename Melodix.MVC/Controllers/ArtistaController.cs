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
  /// Controlador para panel de artista - perfil, subir música, ver estadísticas
  /// Modelos principales: ApplicationUser (artistas), Pista, Album, Genero, Tag
  /// </summary>
  [Authorize(Roles = "Musico")]
  public class ArtistaController : Controller
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ArtistaController> _logger;

    public ArtistaController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IWebHostEnvironment environment,
        ILogger<ArtistaController> logger)
    {
      _userManager = userManager;
      _context = context;
      _environment = environment;
      _logger = logger;
    }

    /// <summary>
    /// Panel principal del artista
    /// </summary>
    public async Task<IActionResult> Dashboard()
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return RedirectToAction("Login", "Cuenta");
      }

      // Estadísticas básicas basadas en archivos subidos por el usuario
      var archivosMusica = await _context.ArchivosSubidos
          .Where(a => a.UsuarioId == usuario.Id && a.Tipo == TipoArchivo.Musica)
          .ToListAsync();

      var totalPistas = archivosMusica.Count;

      // Obtener álbumes (temporal, necesita lógica de asociación)
      var totalAlbumes = await _context.Albums.CountAsync(); // Temporal hasta implementar lógica de artista

      // Reproducciones recientes
      var totalReproduccionesMes = await _context.HistorialesEscucha
          .Where(h => h.EscuchadaEn >= DateTime.UtcNow.AddMonths(-1))
          .CountAsync();

      // Likes recientes
      var totalLikes = await _context.HistorialesLike
          .Where(l => l.AccionLike == AccionLike.Like && l.Fecha >= DateTime.UtcNow.AddMonths(-1))
          .CountAsync();

      // Por ahora, listas vacías hasta implementar la lógica de asociación artista-pista
      var ultimasPistas = new List<Pista>();
      var pistasPopulares = new List<Pista>();

      var viewModel = new ArtistaDashboardViewModel
      {
        Artista = usuario,
        TotalPistas = totalPistas,
        TotalAlbumes = totalAlbumes,
        TotalReproduccionesMes = totalReproduccionesMes,
        TotalLikes = totalLikes,
        UltimasPistas = ultimasPistas,
        PistasPopulares = pistasPopulares
      };

      return View(viewModel);
    }

    /// <summary>
    /// Lista de todas las pistas del artista
    /// </summary>
    public async Task<IActionResult> MisPistas(int pagina = 1, string buscar = "")
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return RedirectToAction("Login", "Cuenta");
      }

      const int pistasPerPage = 20;

      var query = _context.Pistas
          .Include(p => p.Album)
          .Include(p => p.Genero)
          .Where(p => p.UsuarioId == usuario.Id);

      if (!string.IsNullOrEmpty(buscar))
      {
        query = query.Where(p => p.Titulo.Contains(buscar) ||
                               p.Album.Titulo.Contains(buscar));
      }

      var totalPistas = await query.CountAsync();
      var pistas = await query
          .OrderByDescending(p => p.FechaSubida)
          .Skip((pagina - 1) * pistasPerPage)
          .Take(pistasPerPage)
          .ToListAsync();

      var viewModel = new MisPistasViewModel
      {
        Pistas = pistas,
        PaginaActual = pagina,
        TotalPaginas = (int)Math.Ceiling(totalPistas / (double)pistasPerPage),
        TerminoBusqueda = buscar,
        TotalResultados = totalPistas
      };

      return View(viewModel);
    }

    /// <summary>
    /// Lista de álbumes del artista
    /// </summary>
    public async Task<IActionResult> MisAlbumes()
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return RedirectToAction("Login", "Cuenta");
      }

      var albumes = await _context.Albums
          .Include(a => a.Pistas)
          .Where(a => a.UsuarioId == usuario.Id)
          .OrderByDescending(a => a.FechaLanzamiento)
          .ToListAsync();

      return View(albumes);
    }

    /// <summary>
    /// Formulario para subir nueva pista
    /// </summary>
    public async Task<IActionResult> SubirPista()
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return RedirectToAction("Login", "Cuenta");
      }

      // Cargar géneros y álbumes del artista
      var generos = await _context.Generos.ToListAsync();
      var misAlbumes = await _context.Albums
          .Where(a => a.UsuarioId == usuario.Id)
          .ToListAsync();

      ViewBag.Generos = generos;
      ViewBag.MisAlbumes = misAlbumes;

      return View(new SubirPistaViewModel());
    }

    /// <summary>
    /// Procesar subida de nueva pista
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubirPista(SubirPistaViewModel model)
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return RedirectToAction("Login", "Cuenta");
      }

      if (!ModelState.IsValid)
      {
        // Recargar datos para la vista
        ViewBag.Generos = await _context.Generos.ToListAsync();
        ViewBag.MisAlbumes = await _context.Albums
            .Where(a => a.UsuarioId == usuario.Id)
            .ToListAsync();
        return View(model);
      }

      try
      {
        // Validar archivos
        if (model.ArchivoAudio == null || model.ArchivoAudio.Length == 0)
        {
          ModelState.AddModelError("ArchivoAudio", "Debe seleccionar un archivo de audio");
          return View(model);
        }

        // Crear directorios si no existen
        var uploadsPath = Path.Combine(_environment.WebRootPath, "subidos", "audios");
        var imagenesPath = Path.Combine(_environment.WebRootPath, "subidos", "imagenes");
        Directory.CreateDirectory(uploadsPath);
        Directory.CreateDirectory(imagenesPath);

        // Guardar archivo de audio
        var audioFileName = $"{Guid.NewGuid()}_{model.ArchivoAudio.FileName}";
        var audioPath = Path.Combine(uploadsPath, audioFileName);

        using (var stream = new FileStream(audioPath, FileMode.Create))
        {
          await model.ArchivoAudio.CopyToAsync(stream);
        }

        // Guardar imagen si se proporcionó
        string? imagenFileName = null;
        if (model.ArchivoImagen != null && model.ArchivoImagen.Length > 0)
        {
          imagenFileName = $"{Guid.NewGuid()}_{model.ArchivoImagen.FileName}";
          var imagenPath = Path.Combine(imagenesPath, imagenFileName);

          using (var stream = new FileStream(imagenPath, FileMode.Create))
          {
            await model.ArchivoImagen.CopyToAsync(stream);
          }
        }

        // Crear nueva pista
        var nuevaPista = new Pista
        {
          Titulo = model.Titulo,
          UsuarioId = usuario.Id,
          AlbumId = model.AlbumId ?? 0,
          GeneroId = model.GeneroId,
          Duracion = model.Duracion,
          RutaArchivo = $"/subidos/audios/{audioFileName}",
          RutaImagen = imagenFileName != null ? $"/subidos/imagenes/{imagenFileName}" : null,
          FechaSubida = DateTime.UtcNow,
          CreadoEn = DateTime.UtcNow,
          ActualizadoEn = DateTime.UtcNow,
          EsExplicita = model.EsExplicita,
          ContadorReproducciones = 0
        };

        _context.Pistas.Add(nuevaPista);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Pista subida exitosamente";
        return RedirectToAction("MisPistas");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al subir pista para usuario {UserId}", usuario.Id);
        ModelState.AddModelError("", "Error al subir la pista. Inténtalo de nuevo.");

        ViewBag.Generos = await _context.Generos.ToListAsync();
        ViewBag.MisAlbumes = await _context.Albums
            .Where(a => a.UsuarioId == usuario.Id)
            .ToListAsync();

        return View(model);
      }
    }

    /// <summary>
    /// Editar pista existente
    /// </summary>
    public async Task<IActionResult> EditarPista(int id)
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return RedirectToAction("Login", "Cuenta");
      }

      var pista = await _context.Pistas
          .Include(p => p.Album)
          .Include(p => p.Genero)
          .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuario.Id);

      if (pista == null)
      {
        return NotFound();
      }

      var viewModel = new EditarPistaViewModel
      {
        Id = pista.Id,
        Titulo = pista.Titulo,
        AlbumId = pista.AlbumId,
        GeneroId = pista.GeneroId ?? 0,
        Duracion = pista.Duracion,
        EsExplicita = pista.EsExplicita,
        RutaImagenActual = pista.RutaImagen
      };

      ViewBag.Generos = await _context.Generos.ToListAsync();
      ViewBag.MisAlbumes = await _context.Albums
          .Where(a => a.UsuarioId == usuario.Id)
          .ToListAsync();

      return View(viewModel);
    }

    /// <summary>
    /// Procesar edición de pista
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarPista(EditarPistaViewModel model)
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return RedirectToAction("Login", "Cuenta");
      }

      var pista = await _context.Pistas
          .FirstOrDefaultAsync(p => p.Id == model.Id && p.UsuarioId == usuario.Id);

      if (pista == null)
      {
        return NotFound();
      }

      if (!ModelState.IsValid)
      {
        ViewBag.Generos = await _context.Generos.ToListAsync();
        ViewBag.MisAlbumes = await _context.Albums
            .Where(a => a.UsuarioId == usuario.Id)
            .ToListAsync();
        return View(model);
      }

      try
      {
        // Actualizar datos básicos
        pista.Titulo = model.Titulo;
        pista.AlbumId = model.AlbumId ?? 0;
        pista.GeneroId = model.GeneroId;
        pista.Duracion = model.Duracion;
        pista.EsExplicita = model.EsExplicita;

        // Actualizar imagen si se subió una nueva
        if (model.NuevaImagen != null && model.NuevaImagen.Length > 0)
        {
          var imagenesPath = Path.Combine(_environment.WebRootPath, "subidos", "imagenes");
          Directory.CreateDirectory(imagenesPath);

          var imagenFileName = $"{Guid.NewGuid()}_{model.NuevaImagen.FileName}";
          var imagenPath = Path.Combine(imagenesPath, imagenFileName);

          using (var stream = new FileStream(imagenPath, FileMode.Create))
          {
            await model.NuevaImagen.CopyToAsync(stream);
          }

          // Eliminar imagen anterior si existía
          if (!string.IsNullOrEmpty(pista.RutaImagen))
          {
            var imagenAnteriorPath = Path.Combine(_environment.WebRootPath,
                pista.RutaImagen.TrimStart('/'));
            if (System.IO.File.Exists(imagenAnteriorPath))
            {
              System.IO.File.Delete(imagenAnteriorPath);
            }
          }

          pista.RutaImagen = $"/subidos/imagenes/{imagenFileName}";
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Pista actualizada exitosamente";
        return RedirectToAction("MisPistas");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al editar pista {PistaId}", model.Id);
        ModelState.AddModelError("", "Error al actualizar la pista. Inténtalo de nuevo.");

        ViewBag.Generos = await _context.Generos.ToListAsync();
        ViewBag.MisAlbumes = await _context.Albums
            .Where(a => a.UsuarioId == usuario.Id)
            .ToListAsync();

        return View(model);
      }
    }

    /// <summary>
    /// Eliminar pista
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarPista(int id)
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return Json(new { success = false, message = "No autenticado" });
      }

      var pista = await _context.Pistas
          .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuario.Id);

      if (pista == null)
      {
        return Json(new { success = false, message = "Pista no encontrada" });
      }

      try
      {
        // Eliminar archivos del servidor
        if (!string.IsNullOrEmpty(pista.RutaArchivo))
        {
          var audioPath = Path.Combine(_environment.WebRootPath,
              pista.RutaArchivo.TrimStart('/'));
          if (System.IO.File.Exists(audioPath))
          {
            System.IO.File.Delete(audioPath);
          }
        }

        if (!string.IsNullOrEmpty(pista.RutaImagen))
        {
          var imagenPath = Path.Combine(_environment.WebRootPath,
              pista.RutaImagen.TrimStart('/'));
          if (System.IO.File.Exists(imagenPath))
          {
            System.IO.File.Delete(imagenPath);
          }
        }

        _context.Pistas.Remove(pista);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Pista eliminada exitosamente" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al eliminar pista {PistaId}", id);
        return Json(new { success = false, message = "Error al eliminar la pista" });
      }
    }

    /// <summary>
    /// Estadísticas detalladas del artista
    /// </summary>
    public async Task<IActionResult> Estadisticas()
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return RedirectToAction("Login", "Cuenta");
      }

      // Estadísticas generales
      var estadisticas = new EstadisticasArtistaViewModel
      {
        TotalPistas = await _context.Pistas.CountAsync(p => p.UsuarioId == usuario.Id),
        TotalAlbumes = await _context.Albums.CountAsync(a => a.UsuarioId == usuario.Id),
        TotalSeguidores = 0, // TODO: Implementar sistema de seguidores
        TotalReproduccionesTotales = await _context.HistorialesEscucha
              .Include(h => h.Pista)
              .Where(h => h.Pista.UsuarioId == usuario.Id)
              .CountAsync()
      };

      // Pistas más populares
      estadisticas.PistasMasPopulares = await _context.Pistas
          .Where(p => p.UsuarioId == usuario.Id)
          .OrderByDescending(p => p.ContadorReproducciones)
          .Take(10)
          .ToListAsync();

      // Reproducciones por mes (últimos 6 meses)
      var fechaInicio = DateTime.UtcNow.AddMonths(-6);
      estadisticas.ReproduccionesPorMes = await _context.HistorialesEscucha
          .Include(h => h.Pista)
          .Where(h => h.Pista.UsuarioId == usuario.Id && h.EscuchadaEn >= fechaInicio)
          .GroupBy(h => new { h.EscuchadaEn.Year, h.EscuchadaEn.Month })
          .Select(g => new ReproduccionesMesViewModel
          {
            Año = g.Key.Year,
            Mes = g.Key.Month,
            Cantidad = g.Count()
          })
          .OrderBy(r => r.Año).ThenBy(r => r.Mes)
          .ToListAsync();

      return View(estadisticas);
    }

    /// <summary>
    /// Crear nuevo álbum
    /// </summary>
    public IActionResult CrearAlbum()
    {
      return View(new CrearAlbumViewModel());
    }

    /// <summary>
    /// Procesar creación de álbum
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearAlbum(CrearAlbumViewModel model)
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return RedirectToAction("Login", "Cuenta");
      }

      if (!ModelState.IsValid)
      {
        return View(model);
      }

      try
      {
        // Guardar imagen si se proporcionó
        string? imagenFileName = null;
        if (model.ArchivoImagen != null && model.ArchivoImagen.Length > 0)
        {
          var imagenesPath = Path.Combine(_environment.WebRootPath, "subidos", "imagenes");
          Directory.CreateDirectory(imagenesPath);

          imagenFileName = $"{Guid.NewGuid()}_{model.ArchivoImagen.FileName}";
          var imagenPath = Path.Combine(imagenesPath, imagenFileName);

          using (var stream = new FileStream(imagenPath, FileMode.Create))
          {
            await model.ArchivoImagen.CopyToAsync(stream);
          }
        }

        var nuevoAlbum = new Album
        {
          Titulo = model.Titulo,
          UsuarioId = usuario.Id,
          FechaLanzamiento = model.FechaLanzamiento,
          Descripcion = model.Descripcion,
          RutaImagen = imagenFileName != null ? $"/subidos/imagenes/{imagenFileName}" : null
        };

        _context.Albums.Add(nuevoAlbum);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Álbum creado exitosamente";
        return RedirectToAction("MisAlbumes");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al crear álbum para usuario {UserId}", usuario.Id);
        ModelState.AddModelError("", "Error al crear el álbum. Inténtalo de nuevo.");
        return View(model);
      }
    }
  }
}
