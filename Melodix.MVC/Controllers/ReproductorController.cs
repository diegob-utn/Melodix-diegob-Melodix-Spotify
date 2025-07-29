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
  [Authorize]
  public class ReproductorController : Controller
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReproductorController> _logger;

    public ReproductorController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ILogger<ReproductorController> logger)
    {
      _userManager = userManager;
      _context = context;
      _logger = logger;
    }

    /// <summary>
    /// Reproducir una pista específica
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReproducirPista([FromBody] ReproducirPistaRequest request)
    {
      var pista = await _context.Pistas
          .Include(p => p.Album)
          .Include(p => p.Usuario)
          .FirstOrDefaultAsync(p => p.Id == request.PistaId);

      if (pista == null)
      {
        return Json(new { success = false, message = "Pista no encontrada" });
      }

      // Verificar si el usuario tiene permisos para acceder a la pista
      // (por ahora todas son públicas, pero podría implementarse lógica de suscripción)

      var resultado = new
      {
        success = true,
        pista = new
        {
          id = pista.Id,
          titulo = pista.Titulo,
          duracion = 180, // Default duration in seconds since Duracion field was removed
          urlPortada = pista.UrlPortada ?? "/images/default-album.png",
          album = new
          {
            id = pista.Album?.Id ?? 0,
            titulo = pista.Album?.Titulo ?? "Desconocido"
          },
          // URL del archivo de audio real
          urlAudio = pista.RutaArchivo ?? pista.UrlArchivo ?? $"/uploads/audio/{pista.Id}.mp3"
        }
      };

      return Json(resultado);
    }

    /// <summary>
    /// Reproducir una pista específica
    /// </summary>
    public async Task<IActionResult> Reproducir(int pistaId, int? listaId = null)
    {
      var pista = await _context.Pistas
          .Include(p => p.Album)
          .FirstOrDefaultAsync(p => p.Id == pistaId);

      if (pista == null)
      {
        return NotFound("Pista no encontrada");
      }

      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return RedirectToAction("Login", "Cuenta");
      }

      // Registrar en historial de escucha
      await RegistrarEscucha(usuario.Id, pistaId);

      var viewModel = new ReproductorViewModel
      {
        PistaActual = pista,
        IndiceActual = 0,
        Volumen = 0.5
      };

      // Si viene de una lista, cargar toda la lista como cola de reproducción
      if (listaId.HasValue)
      {
        var lista = await _context.ListasReproduccion
            .Include(l => l.ListasPista)
                .ThenInclude(lp => lp.Pista)
                    .ThenInclude(p => p.Album)
            .FirstOrDefaultAsync(l => l.Id == listaId.Value);

        if (lista != null)
        {
          viewModel.Cola = lista.ListasPista
              .OrderBy(lp => lp.Posicion)
              .Select(lp => lp.Pista)
              .ToList();

          viewModel.IndiceActual = viewModel.Cola.FindIndex(p => p.Id == pistaId);
        }
      }
      else
      {
        // Si no viene de lista, agregar solo esta pista
        viewModel.Cola = new List<Pista> { pista };
      }

      return View(viewModel);
    }

    /// <summary>
    /// API para obtener información de una pista para el reproductor
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerPista(int pistaId)
    {
      var pista = await _context.Pistas
          .Include(p => p.Album)
          .FirstOrDefaultAsync(p => p.Id == pistaId);

      if (pista == null)
      {
        return Json(new { success = false, message = "Pista no encontrada" });
      }

      // Verificar si el usuario tiene permisos para acceder a la pista
      // (por ahora todas son públicas, pero podría implementarse lógica de suscripción)

      var resultado = new
      {
        success = true,
        pista = new
        {
          id = pista.Id,
          titulo = pista.Titulo,
          duracion = 180, // Default duration in seconds since Duracion field was removed
          urlPortada = pista.UrlPortada ?? "/images/default-album.png",
          album = new
          {
            id = pista.Album?.Id ?? 0,
            titulo = pista.Album?.Titulo ?? "Desconocido"
          },
          // URL del archivo de audio real
          urlAudio = pista.RutaArchivo ?? pista.UrlArchivo ?? $"/uploads/audio/{pista.Id}.mp3"
        }
      };

      return Json(resultado);
    }

    /// <summary>
    /// Registrar que una pista fue escuchada
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarEscucha(int pistaId)
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return Json(new { success = false, message = "No autenticado" });
      }

      await RegistrarEscucha(usuario.Id, pistaId);
      return Json(new { success = true });
    }

    /// <summary>
    /// Obtener siguiente pista en la cola
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SiguientePista(int pistaActualId, int? listaId = null, bool esAleatorio = false)
    {
      Pista? siguientePista = null;

      if (listaId.HasValue)
      {
        var lista = await _context.ListasReproduccion
            .Include(l => l.ListasPista)
                .ThenInclude(lp => lp.Pista)
                    .ThenInclude(p => p.Album)
            .FirstOrDefaultAsync(l => l.Id == listaId.Value);

        if (lista != null)
        {
          var pistasOrdenadas = lista.ListasPista
              .OrderBy(lp => lp.Posicion)
              .Select(lp => lp.Pista)
              .ToList();

          if (esAleatorio)
          {
            var random = new Random();
            var pistasDisponibles = pistasOrdenadas.Where(p => p.Id != pistaActualId).ToList();
            if (pistasDisponibles.Any())
            {
              siguientePista = pistasDisponibles[random.Next(pistasDisponibles.Count)];
            }
          }
          else
          {
            var indiceActual = pistasOrdenadas.FindIndex(p => p.Id == pistaActualId);
            if (indiceActual >= 0 && indiceActual < pistasOrdenadas.Count - 1)
            {
              siguientePista = pistasOrdenadas[indiceActual + 1];
            }
          }
        }
      }

      if (siguientePista == null)
      {
        return Json(new { success = false, message = "No hay siguiente pista" });
      }

      var resultado = new
      {
        success = true,
        pista = new
        {
          id = siguientePista.Id,
          titulo = siguientePista.Titulo,
          duracion = 180, // Default duration in seconds since Duracion field was removed
          urlPortada = siguientePista.UrlPortada ?? "/images/default-album.png",
          album = new
          {
            id = siguientePista.Album?.Id ?? 0,
            titulo = siguientePista.Album?.Titulo ?? "Desconocido"
          },
          urlAudio = siguientePista.RutaArchivo ?? siguientePista.UrlArchivo ?? $"/uploads/audio/{siguientePista.Id}.mp3"
        }
      };

      return Json(resultado);
    }

    /// <summary>
    /// Obtener pista anterior en la cola
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> PistaAnterior(int pistaActualId, int? listaId = null)
    {
      Pista? pistaAnterior = null;

      if (listaId.HasValue)
      {
        var lista = await _context.ListasReproduccion
            .Include(l => l.ListasPista)
                .ThenInclude(lp => lp.Pista)
                    .ThenInclude(p => p.Album)
            .FirstOrDefaultAsync(l => l.Id == listaId.Value);

        if (lista != null)
        {
          var pistasOrdenadas = lista.ListasPista
              .OrderBy(lp => lp.Posicion)
              .Select(lp => lp.Pista)
              .ToList();

          var indiceActual = pistasOrdenadas.FindIndex(p => p.Id == pistaActualId);
          if (indiceActual > 0)
          {
            pistaAnterior = pistasOrdenadas[indiceActual - 1];
          }
        }
      }

      if (pistaAnterior == null)
      {
        return Json(new { success = false, message = "No hay pista anterior" });
      }

      var resultado = new
      {
        success = true,
        pista = new
        {
          id = pistaAnterior.Id,
          titulo = pistaAnterior.Titulo,
          duracion = 180, // Default duration in seconds since Duracion field was removed
          urlPortada = pistaAnterior.UrlPortada ?? "/images/default-album.png",
          album = new
          {
            id = pistaAnterior.Album?.Id ?? 0,
            titulo = pistaAnterior.Album?.Titulo ?? "Desconocido"
          },
          urlAudio = pistaAnterior.RutaArchivo ?? pistaAnterior.UrlArchivo ?? $"/uploads/audio/{pistaAnterior.Id}.mp3"
        }
      };

      return Json(resultado);
    }

    /// <summary>
    /// Crear cola de reproducción a partir de un álbum
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReproducirAlbum(int albumId, int? pistaInicial = null)
    {
      var album = await _context.Albums
          .Include(a => a.Pistas)
          .FirstOrDefaultAsync(a => a.Id == albumId);

      if (album == null)
      {
        return Json(new { success = false, message = "Álbum no encontrado" });
      }

      var pistasOrdenadas = album.Pistas.OrderBy(p => p.Id).ToList(); // O cualquier otro criterio de orden

      var pistaParaReproducir = pistaInicial.HasValue
          ? pistasOrdenadas.FirstOrDefault(p => p.Id == pistaInicial.Value)
          : pistasOrdenadas.FirstOrDefault();

      if (pistaParaReproducir == null)
      {
        return Json(new { success = false, message = "No hay pistas en el álbum" });
      }

      var url = Url.Action("Reproducir", new { pistaId = pistaParaReproducir.Id, albumId = albumId });
      return Json(new { success = true, redirectUrl = url });
    }

    /// <summary>
    /// Obtener historial de reproducción del usuario
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Historial(int pagina = 1, int cantidad = 20)
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return RedirectToAction("Login", "Cuenta");
      }

      var historial = await _context.HistorialesEscucha
          .Include(h => h.Pista)
              .ThenInclude(p => p.Album)
          .Where(h => h.UsuarioId == usuario.Id)
          .OrderByDescending(h => h.EscuchadaEn)
          .Skip((pagina - 1) * cantidad)
          .Take(cantidad)
          .ToListAsync();

      return View(historial);
    }

    /// <summary>
    /// Método privado para registrar escucha en la base de datos
    /// </summary>
    private async Task RegistrarEscucha(string usuarioId, int pistaId)
    {
      // Evitar registrar múltiples escuchas muy rápidas de la misma pista
      var escuchaReciente = await _context.HistorialesEscucha
          .Where(h => h.UsuarioId == usuarioId &&
                     h.PistaId == pistaId &&
                     h.EscuchadaEn >= DateTime.UtcNow.AddMinutes(-2))
          .FirstOrDefaultAsync();

      if (escuchaReciente == null)
      {
        var historial = new HistorialEscucha
        {
          UsuarioId = usuarioId,
          PistaId = pistaId,
          EscuchadaEn = DateTime.UtcNow
        };

        _context.HistorialesEscucha.Add(historial);
        await _context.SaveChangesAsync();
      }
    }
  }
}

/// <summary>
/// Modelo de request para reproducir pista
/// </summary>
public class ReproducirPistaRequest
{
  public int PistaId { get; set; }
}
