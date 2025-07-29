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
  /// Controlador para mostrar contenido guardado por el usuario (listas, favoritos)
  /// Modelos principales: ListaReproduccion, UsuarioLikePista, UsuarioLikeAlbum, UsuarioSigueLista
  /// </summary>
  [Authorize]
  public class BibliotecaController : Controller
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BibliotecaController> _logger;

    public BibliotecaController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ILogger<BibliotecaController> logger)
    {
      _userManager = userManager;
      _context = context;
      _logger = logger;
    }

    public async Task<IActionResult> Index(string filtro = "todo")
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return RedirectToAction("Login", "Cuenta");
      }

      var viewModel = new BibliotecaViewModel
      {
        UsuarioActual = usuario
      };

      switch (filtro.ToLower())
      {
        case "listas":
          viewModel.MisListas = await ObtenerMisListas(usuario.Id);
          break;

        case "albums":
          viewModel.AlbumsGuardados = await ObtenerAlbumsGuardados(usuario.Id);
          break;

        case "pistas":
          viewModel.PistasGuardadas = await ObtenerPistasGuardadas(usuario.Id);
          break;

        case "artistas":
          viewModel.ArtistasSeguidores = await ObtenerArtistasSeguidores(usuario.Id);
          break;

        default: // "todo"
          viewModel.MisListas = await ObtenerMisListas(usuario.Id);
          viewModel.AlbumsGuardados = await ObtenerAlbumsGuardados(usuario.Id, 6);
          viewModel.PistasGuardadas = await ObtenerPistasGuardadas(usuario.Id, 8);
          viewModel.ArtistasSeguidores = await ObtenerArtistasSeguidores(usuario.Id, 6);
          break;
      }

      ViewBag.FiltroActual = filtro;
      return View(viewModel);
    }

    private async Task<List<ListaReproduccion>> ObtenerMisListas(string usuarioId, int? limite = null)
    {
      var queryOrdered = _context.ListasReproduccion
          .Include(l => l.ListasPista)
              .ThenInclude(lp => lp.Pista)
                  .ThenInclude(p => p.Album)
          .Where(l => l.UsuarioId == usuarioId)
          .OrderByDescending(l => l.ActualizadoEn);

      if (limite.HasValue)
      {
        return await queryOrdered.Take(limite.Value).ToListAsync();
      }

      return await queryOrdered.ToListAsync();
    }

    private async Task<List<Album>> ObtenerAlbumsGuardados(string usuarioId, int? limite = null)
    {
      var query = _context.UsuariosLikeAlbum
          .Include(ula => ula.Album)
              .ThenInclude(a => a.Pistas)
          .Where(ula => ula.UsuarioId == usuarioId)
          .OrderByDescending(ula => ula.CreadoEn)
          .Select(ula => ula.Album);

      if (limite.HasValue)
      {
        query = query.Take(limite.Value);
      }

      return await query.ToListAsync();
    }

    private async Task<List<Pista>> ObtenerPistasGuardadas(string usuarioId, int? limite = null)
    {
      var query = _context.UsuariosLikePista
          .Include(ulp => ulp.Pista)
              .ThenInclude(p => p.Album)
          .Where(ulp => ulp.UsuarioId == usuarioId)
          .OrderByDescending(ulp => ulp.CreadoEn)
          .Select(ulp => ulp.Pista);

      if (limite.HasValue)
      {
        query = query.Take(limite.Value);
      }

      return await query.ToListAsync();
    }

    private async Task<List<ApplicationUser>> ObtenerArtistasSeguidores(string usuarioId, int? limite = null)
    {
      var query = _context.UsuariosSigue
          .Include(us => us.Seguido)
          .Where(us => us.SeguidorId == usuarioId &&
                      (us.Seguido.Rol == RolUsuario.Musico || us.Seguido.Rol == RolUsuario.Admin))
          .OrderByDescending(us => us.CreadoEn)
          .Select(us => us.Seguido);

      if (limite.HasValue)
      {
        query = query.Take(limite.Value);
      }

      return await query.ToListAsync();
    }

    /// <summary>
    /// Crear nueva lista de reproducción
    /// </summary>
    [HttpGet]
    public IActionResult CrearLista()
    {
      return View(new PlaylistViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearLista(PlaylistViewModel model)
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

      var nuevaLista = new ListaReproduccion
      {
        Nombre = model.Nombre,
        Descripcion = model.Descripcion ?? string.Empty,
        Publica = model.Publica,
        Colaborativa = model.Colaborativa,
        UsuarioId = usuario.Id,
        CreadoEn = DateTime.UtcNow,
        ActualizadoEn = DateTime.UtcNow,
        SpotifyListaId = string.Empty, // Se puede generar un GUID si es necesario
        Sincronizada = false
      };

      _context.ListasReproduccion.Add(nuevaLista);
      await _context.SaveChangesAsync();

      TempData["Success"] = "Lista de reproducción creada exitosamente";
      return RedirectToAction("Detalle", "Playlist", new { id = nuevaLista.Id });
    }

    /// <summary>
    /// Eliminar lista de reproducción
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarLista(int id)
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return Json(new { success = false, message = "No autenticado" });
      }

      var lista = await _context.ListasReproduccion
          .FirstOrDefaultAsync(l => l.Id == id && l.UsuarioId == usuario.Id);

      if (lista == null)
      {
        return Json(new { success = false, message = "Lista no encontrada o no tienes permisos" });
      }

      // Eliminar las relaciones con pistas
      var listasPista = await _context.ListasPista
          .Where(lp => lp.ListaId == id)
          .ToListAsync();

      _context.ListasPista.RemoveRange(listasPista);

      // Eliminar likes de la lista
      var likes = await _context.UsuariosLikeLista
          .Where(ull => ull.ListaId == id)
          .ToListAsync();

      _context.UsuariosLikeLista.RemoveRange(likes);

      // Eliminar seguidores de la lista
      var seguidores = await _context.UsuariosSigueLista
          .Where(usl => usl.ListaId == id)
          .ToListAsync();

      _context.UsuariosSigueLista.RemoveRange(seguidores);

      // Eliminar la lista
      _context.ListasReproduccion.Remove(lista);
      await _context.SaveChangesAsync();

      return Json(new { success = true, message = "Lista eliminada exitosamente" });
    }

    /// <summary>
    /// Quitar pista de favoritos
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuitarDeFavoritos(int pistaId)
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return Json(new { success = false, message = "No autenticado" });
      }

      var like = await _context.UsuariosLikePista
          .FirstOrDefaultAsync(ulp => ulp.UsuarioId == usuario.Id && ulp.PistaId == pistaId);

      if (like == null)
      {
        return Json(new { success = false, message = "La pista no está en favoritos" });
      }

      _context.UsuariosLikePista.Remove(like);

      // Registrar en historial
      var historial = new HistorialLike
      {
        UsuarioId = usuario.Id,
        TipoObjetoLike = TipoObjetoLike.Pista,
        ObjetoId = pistaId,
        AccionLike = AccionLike.Unlike,
        Fecha = DateTime.UtcNow
      };
      _context.HistorialesLike.Add(historial);

      await _context.SaveChangesAsync();

      return Json(new { success = true, message = "Pista quitada de favoritos" });
    }

    /// <summary>
    /// Quitar álbum de guardados
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuitarAlbumGuardado(int albumId)
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return Json(new { success = false, message = "No autenticado" });
      }

      var like = await _context.UsuariosLikeAlbum
          .FirstOrDefaultAsync(ula => ula.UsuarioId == usuario.Id && ula.AlbumId == albumId);

      if (like == null)
      {
        return Json(new { success = false, message = "El álbum no está guardado" });
      }

      _context.UsuariosLikeAlbum.Remove(like);

      // Registrar en historial
      var historial = new HistorialLike
      {
        UsuarioId = usuario.Id,
        TipoObjetoLike = TipoObjetoLike.Album,
        ObjetoId = albumId,
        AccionLike = AccionLike.Unlike,
        Fecha = DateTime.UtcNow
      };
      _context.HistorialesLike.Add(historial);

      await _context.SaveChangesAsync();

      return Json(new { success = true, message = "Álbum quitado de guardados" });
    }
  }
}
