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
  /// Controlador para buscar canciones, artistas, álbumes, listas
  /// Modelos principales: Pista, Album, ListaReproduccion, ApplicationUser
  /// </summary>
  [Authorize]
  public class BusquedaController : Controller
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BusquedaController> _logger;

    public BusquedaController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ILogger<BusquedaController> logger)
    {
      _userManager = userManager;
      _context = context;
      _logger = logger;
    }

    public async Task<IActionResult> Index(string? query)
    {
      var viewModel = new BusquedaViewModel { Query = query ?? string.Empty };

      if (!string.IsNullOrWhiteSpace(query))
      {
        var queryLower = query.ToLower();

        // Buscar pistas
        viewModel.Pistas = await _context.Pistas
            .Include(p => p.Album)
            .Where(p => p.Titulo.ToLower().Contains(queryLower))
            .OrderByDescending(p => p.CreadoEn)
            .Take(20)
            .ToListAsync();

        // Buscar álbumes
        viewModel.Albums = await _context.Albums
            .Include(a => a.Pistas)
            .Where(a => a.Titulo.ToLower().Contains(queryLower))
            .OrderByDescending(a => a.FechaLanzamiento)
            .Take(10)
            .ToListAsync();

        // Buscar listas de reproducción públicas
        viewModel.Listas = await _context.ListasReproduccion
            .Include(l => l.Usuario)
            .Include(l => l.ListasPista)
                .ThenInclude(lp => lp.Pista)
            .Where(l => l.Publica && l.Nombre.ToLower().Contains(queryLower))
            .OrderByDescending(l => l.CreadoEn)
            .Take(10)
            .ToListAsync();

        // Buscar usuarios (artistas/usuarios públicos)
        viewModel.Usuarios = await _context.Users
            .Where(u => (u.Nombre.ToLower().Contains(queryLower) ||
                        u.Nick.ToLower().Contains(queryLower)) &&
                        u.Activo)
            .OrderBy(u => u.Nombre)
            .Take(10)
            .ToListAsync();
      }

      return View(viewModel);
    }

    /// <summary>
    /// Búsqueda con autocompletado para AJAX
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Autocompletar(string term)
    {
      if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
      {
        return Json(new List<object>());
      }

      var termLower = term.ToLower();
      var sugerencias = new List<object>();

      // Agregar pistas
      var pistas = await _context.Pistas
          .Include(p => p.Album)
          .Where(p => p.Titulo.ToLower().Contains(termLower))
          .Take(5)
          .Select(p => new
          {
            tipo = "pista",
            id = p.Id,
            titulo = p.Titulo,
            subtitulo = p.Album.Titulo,
            imagen = p.UrlPortada,
            url = Url.Action("Reproducir", "Reproductor", new { pistaId = p.Id })
          })
          .ToListAsync();

      // Agregar álbumes
      var albums = await _context.Albums
          .Where(a => a.Titulo.ToLower().Contains(termLower))
          .Take(3)
          .Select(a => new
          {
            tipo = "album",
            id = a.Id,
            titulo = a.Titulo,
            subtitulo = "Álbum",
            imagen = a.UrlPortada,
            url = Url.Action("Detalle", "Album", new { id = a.Id })
          })
          .ToListAsync();

      // Agregar listas
      var listas = await _context.ListasReproduccion
          .Include(l => l.Usuario)
          .Where(l => l.Publica && l.Nombre.ToLower().Contains(termLower))
          .Take(3)
          .Select(l => new
          {
            tipo = "lista",
            id = l.Id,
            titulo = l.Nombre,
            subtitulo = $"Por {l.Usuario.Nombre}",
            imagen = "/images/playlist-default.png",
            url = Url.Action("Detalle", "Playlist", new { id = l.Id })
          })
          .ToListAsync();

      // Agregar usuarios
      var usuarios = await _context.Users
          .Where(u => (u.Nombre.ToLower().Contains(termLower) ||
                      u.Nick.ToLower().Contains(termLower)) &&
                      u.Activo)
          .Take(3)
          .Select(u => new
          {
            tipo = "usuario",
            id = u.Id,
            titulo = u.Nombre,
            subtitulo = $"@{u.Nick}",
            imagen = u.FotoPerfil ?? "/images/user-default.png",
            url = Url.Action("Index", "Perfil", new { nick = u.Nick })
          })
          .ToListAsync();

      sugerencias.AddRange(pistas);
      sugerencias.AddRange(albums);
      sugerencias.AddRange(listas);
      sugerencias.AddRange(usuarios);

      return Json(sugerencias.Take(10));
    }

    /// <summary>
    /// Búsqueda filtrada por tipo
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Filtrar(string query, string tipo = "todo", int pagina = 1, int cantidad = 20)
    {
      if (string.IsNullOrWhiteSpace(query))
      {
        return RedirectToAction("Index");
      }

      var viewModel = new BusquedaViewModel { Query = query };
      var queryLower = query.ToLower();

      switch (tipo.ToLower())
      {
        case "pistas":
          viewModel.Pistas = await _context.Pistas
              .Include(p => p.Album)
              .Where(p => p.Titulo.ToLower().Contains(queryLower))
              .OrderByDescending(p => p.CreadoEn)
              .Skip((pagina - 1) * cantidad)
              .Take(cantidad)
              .ToListAsync();
          break;

        case "albums":
          viewModel.Albums = await _context.Albums
              .Include(a => a.Pistas)
              .Where(a => a.Titulo.ToLower().Contains(queryLower))
              .OrderByDescending(a => a.FechaLanzamiento)
              .Skip((pagina - 1) * cantidad)
              .Take(cantidad)
              .ToListAsync();
          break;

        case "listas":
          viewModel.Listas = await _context.ListasReproduccion
              .Include(l => l.Usuario)
              .Include(l => l.ListasPista)
                  .ThenInclude(lp => lp.Pista)
              .Where(l => l.Publica && l.Nombre.ToLower().Contains(queryLower))
              .OrderByDescending(l => l.CreadoEn)
              .Skip((pagina - 1) * cantidad)
              .Take(cantidad)
              .ToListAsync();
          break;

        case "usuarios":
          viewModel.Usuarios = await _context.Users
              .Where(u => (u.Nombre.ToLower().Contains(queryLower) ||
                          u.Nick.ToLower().Contains(queryLower)) &&
                          u.Activo)
              .OrderBy(u => u.Nombre)
              .Skip((pagina - 1) * cantidad)
              .Take(cantidad)
              .ToListAsync();
          break;

        default:
          // Búsqueda completa (como en Index)
          return await Index(query);
      }

      ViewBag.TipoFiltro = tipo;
      ViewBag.PaginaActual = pagina;

      return View("Index", viewModel);
    }
  }
}
