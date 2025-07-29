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
  /// Controlador para el feed musical con recomendaciones, álbumes y listas destacadas
  /// Modelos principales: Album, ListaReproduccion, Pista
  /// </summary>
  [Authorize]
  public class InicioController : Controller
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InicioController> _logger;

    public InicioController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ILogger<InicioController> logger)
    {
      _userManager = userManager;
      _context = context;
      _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return RedirectToAction("Login", "Cuenta");
      }

      var viewModel = new InicioViewModel
      {
        UsuarioActual = usuario,
        AlbumsDestacados = await ObtenerAlbumsDestacados(),
        ListasRecomendadas = await ObtenerListasRecomendadas(usuario.Id),
        PistasRecientes = await ObtenerPistasRecientes(),
        PistasTendencia = await ObtenerPistasTendencia()
      };

      return View(viewModel);
    }

    private async Task<List<Album>> ObtenerAlbumsDestacados()
    {
      return await _context.Albums
          .Include(a => a.Pistas)
          .OrderByDescending(a => a.FechaLanzamiento)
          .Take(6)
          .ToListAsync();
    }

    private async Task<List<ListaReproduccion>> ObtenerListasRecomendadas(string usuarioId)
    {
      // Obtener listas públicas, excluyendo las propias
      return await _context.ListasReproduccion
          .Include(l => l.Usuario)
          .Include(l => l.ListasPista)
              .ThenInclude(lp => lp.Pista)
          .Where(l => l.Publica && l.UsuarioId != usuarioId)
          .OrderByDescending(l => l.CreadoEn)
          .Take(8)
          .ToListAsync();
    }

    private async Task<List<Pista>> ObtenerPistasRecientes()
    {
      return await _context.Pistas
          .Include(p => p.Album)
          .OrderByDescending(p => p.CreadoEn)
          .Take(10)
          .ToListAsync();
    }

    private async Task<List<Pista>> ObtenerPistasTendencia()
    {
      try
      {
        // Obtener pistas más escuchadas en los últimos 30 días
        var fechaLimite = DateTime.UtcNow.AddDays(-30);

        var pistasTendencia = await _context.HistorialesEscucha
            .Where(h => h.EscuchadaEn >= fechaLimite)
            .GroupBy(h => h.PistaId)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => new { PistaId = g.Key, Conteo = g.Count() })
            .ToListAsync();

        if (!pistasTendencia.Any())
        {
          // Si no hay historial, devolver las pistas más recientes
          return await _context.Pistas
              .Include(p => p.Album)
              .Include(p => p.Usuario)
              .OrderByDescending(p => p.CreadoEn)
              .Take(10)
              .ToListAsync();
        }

        var pistaIds = pistasTendencia.Select(pt => pt.PistaId).ToList();
        var pistas = await _context.Pistas
            .Include(p => p.Album)
            .Include(p => p.Usuario)
            .Where(p => pistaIds.Contains(p.Id))
            .ToListAsync();

        // Ordenar según el conteo de escuchas
        return pistas
            .OrderByDescending(p => pistasTendencia.First(pt => pt.PistaId == p.Id).Conteo)
            .ToList();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al obtener pistas de tendencia");

        // Fallback: devolver las pistas más recientes
        return await _context.Pistas
            .Include(p => p.Album)
            .Include(p => p.Usuario)
            .OrderByDescending(p => p.CreadoEn)
            .Take(10)
            .ToListAsync();
      }
    }

    /// <summary>
    /// Acción para marcar/desmarcar like en una pista desde el feed
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLikePista(int pistaId)
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return Json(new { success = false, message = "No autenticado" });
      }

      var pista = await _context.Pistas.FindAsync(pistaId);
      if (pista == null)
      {
        return Json(new { success = false, message = "Pista no encontrada" });
      }

      var existingLike = await _context.UsuariosLikePista
          .FirstOrDefaultAsync(ul => ul.UsuarioId == usuario.Id && ul.PistaId == pistaId);

      bool isLiked;
      if (existingLike != null)
      {
        // Quitar like
        _context.UsuariosLikePista.Remove(existingLike);
        isLiked = false;
      }
      else
      {
        // Agregar like
        var nuevoLike = new UsuarioLikePista
        {
          UsuarioId = usuario.Id,
          PistaId = pistaId,
          CreadoEn = DateTime.UtcNow
        };
        _context.UsuariosLikePista.Add(nuevoLike);
        isLiked = true;
      }

      // Registrar en historial de likes
      var historialLike = new HistorialLike
      {
        UsuarioId = usuario.Id,
        TipoObjetoLike = TipoObjetoLike.Pista,
        ObjetoId = pistaId,
        AccionLike = isLiked ? AccionLike.Like : AccionLike.Unlike,
        Fecha = DateTime.UtcNow
      };
      _context.HistorialesLike.Add(historialLike);

      await _context.SaveChangesAsync();

      return Json(new { success = true, isLiked = isLiked });
    }

    /// <summary>
    /// Acción para obtener más contenido (scroll infinito)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CargarMasContenido(string tipo, int pagina = 0, int cantidad = 6)
    {
      switch (tipo.ToLower())
      {
        case "albums":
          var albums = await _context.Albums
              .Include(a => a.Pistas)
              .OrderByDescending(a => a.FechaLanzamiento)
              .Skip(pagina * cantidad)
              .Take(cantidad)
              .ToListAsync();
          return PartialView("_AlbumsGrid", albums);

        case "listas":
          var usuario = await _userManager.GetUserAsync(User);
          var listas = await _context.ListasReproduccion
              .Include(l => l.Usuario)
              .Where(l => l.Publica && l.UsuarioId != usuario!.Id)
              .OrderByDescending(l => l.CreadoEn)
              .Skip(pagina * cantidad)
              .Take(cantidad)
              .ToListAsync();
          return PartialView("_ListasGrid", listas);

        default:
          return BadRequest("Tipo de contenido no válido");
      }
    }
  }
}
