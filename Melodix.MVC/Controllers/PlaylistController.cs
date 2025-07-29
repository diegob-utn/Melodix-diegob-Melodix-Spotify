using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Melodix.Models;
using Melodix.Models.Models;
using Melodix.Data;
using Melodix.MVC.ViewModels;

namespace Melodix.MVC.Controllers
{
  /// <summary>
  /// Controlador para crear, editar, eliminar y ver listas de reproducción
  /// Modelos principales: ListaReproduccion, ListaPista, Pista, UsuarioSigueLista
  /// </summary>
  [Authorize]
  public class PlaylistController : Controller
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PlaylistController> _logger;

    public PlaylistController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ILogger<PlaylistController> logger)
    {
      _userManager = userManager;
      _context = context;
      _logger = logger;
    }

    /// <summary>
    /// Ver detalles de una playlist específica
    /// </summary>
    public async Task<IActionResult> Detalle(int id)
    {
      var lista = await _context.ListasReproduccion
          .Include(l => l.Usuario)
          .Include(l => l.ListasPista)
              .ThenInclude(lp => lp.Pista)
                  .ThenInclude(p => p.Album)
          .FirstOrDefaultAsync(l => l.Id == id);

      if (lista == null)
      {
        return NotFound("Lista de reproducción no encontrada");
      }

      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return RedirectToAction("Login", "Cuenta");
      }

      // Verificar si el usuario puede ver esta lista
      if (!lista.Publica && lista.UsuarioId != usuario.Id)
      {
        return Forbid("No tienes permisos para ver esta lista");
      }

      var viewModel = new PlaylistViewModel
      {
        Id = lista.Id,
        Nombre = lista.Nombre,
        Descripcion = lista.Descripcion,
        Publica = lista.Publica,
        Colaborativa = lista.Colaborativa,
        Propietario = lista.Usuario,
        EsPropietario = lista.UsuarioId == usuario.Id,
        CreadoEn = lista.CreadoEn,
        Pistas = lista.ListasPista
              .OrderBy(lp => lp.Posicion)
              .Select(lp => lp.Pista)
              .ToList(),
        TotalDuracion = 0 // Duracion field removed, setting to 0
      };

      return View(viewModel);
    }

    /// <summary>
    /// Formulario para crear nueva playlist
    /// </summary>
    [HttpGet]
    public IActionResult Crear()
    {
      return View(new PlaylistViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(PlaylistViewModel model)
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
        SpotifyListaId = string.Empty,
        Sincronizada = false
      };

      _context.ListasReproduccion.Add(nuevaLista);
      await _context.SaveChangesAsync();

      TempData["Success"] = "Lista de reproducción creada exitosamente";
      return RedirectToAction("Detalle", new { id = nuevaLista.Id });
    }

    /// <summary>
    /// Formulario para editar playlist existente
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return RedirectToAction("Login", "Cuenta");
      }

      var lista = await _context.ListasReproduccion
          .FirstOrDefaultAsync(l => l.Id == id && l.UsuarioId == usuario.Id);

      if (lista == null)
      {
        return NotFound("Lista no encontrada o no tienes permisos para editarla");
      }

      var viewModel = new PlaylistViewModel
      {
        Id = lista.Id,
        Nombre = lista.Nombre,
        Descripcion = lista.Descripcion,
        Publica = lista.Publica,
        Colaborativa = lista.Colaborativa
      };

      return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, PlaylistViewModel model)
    {
      if (id != model.Id)
      {
        return BadRequest("ID de lista no coincide");
      }

      if (!ModelState.IsValid)
      {
        return View(model);
      }

      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return RedirectToAction("Login", "Cuenta");
      }

      var lista = await _context.ListasReproduccion
          .FirstOrDefaultAsync(l => l.Id == id && l.UsuarioId == usuario.Id);

      if (lista == null)
      {
        return NotFound("Lista no encontrada o no tienes permisos para editarla");
      }

      lista.Nombre = model.Nombre;
      lista.Descripcion = model.Descripcion ?? string.Empty;
      lista.Publica = model.Publica;
      lista.Colaborativa = model.Colaborativa;
      lista.ActualizadoEn = DateTime.UtcNow;

      await _context.SaveChangesAsync();

      TempData["Success"] = "Lista actualizada exitosamente";
      return RedirectToAction("Detalle", new { id = lista.Id });
    }

    /// <summary>
    /// Eliminar playlist
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
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

      // Eliminar relaciones con pistas
      var listasPista = await _context.ListasPista
          .Where(lp => lp.ListaId == id)
          .ToListAsync();
      _context.ListasPista.RemoveRange(listasPista);

      // Eliminar likes
      var likes = await _context.UsuariosLikeLista
          .Where(ull => ull.ListaId == id)
          .ToListAsync();
      _context.UsuariosLikeLista.RemoveRange(likes);

      // Eliminar seguidores
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
    /// Agregar pista a playlist
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarPista(int listaId, int pistaId)
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return Json(new { success = false, message = "No autenticado" });
      }

      var lista = await _context.ListasReproduccion
          .FirstOrDefaultAsync(l => l.Id == listaId &&
                                  (l.UsuarioId == usuario.Id || l.Colaborativa));

      if (lista == null)
      {
        return Json(new { success = false, message = "Lista no encontrada o no tienes permisos" });
      }

      var pista = await _context.Pistas.FindAsync(pistaId);
      if (pista == null)
      {
        return Json(new { success = false, message = "Pista no encontrada" });
      }

      // Verificar si la pista ya está en la lista
      var yaExiste = await _context.ListasPista
          .AnyAsync(lp => lp.ListaId == listaId && lp.PistaId == pistaId);

      if (yaExiste)
      {
        return Json(new { success = false, message = "La pista ya está en la lista" });
      }

      // Obtener la siguiente posición
      var ultimaPosicion = await _context.ListasPista
          .Where(lp => lp.ListaId == listaId)
          .MaxAsync(lp => (int?)lp.Posicion) ?? 0;

      var nuevaListaPista = new ListaPista
      {
        ListaId = listaId,
        PistaId = pistaId,
        Posicion = ultimaPosicion + 1,
        AgregadoEn = DateTime.UtcNow
      };

      _context.ListasPista.Add(nuevaListaPista);

      // Actualizar fecha de modificación de la lista
      lista.ActualizadoEn = DateTime.UtcNow;

      await _context.SaveChangesAsync();

      return Json(new { success = true, message = "Pista agregada a la lista" });
    }

    /// <summary>
    /// Quitar pista de playlist
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuitarPista(int listaId, int pistaId)
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return Json(new { success = false, message = "No autenticado" });
      }

      var lista = await _context.ListasReproduccion
          .FirstOrDefaultAsync(l => l.Id == listaId &&
                                  (l.UsuarioId == usuario.Id || l.Colaborativa));

      if (lista == null)
      {
        return Json(new { success = false, message = "Lista no encontrada o no tienes permisos" });
      }

      var listaPista = await _context.ListasPista
          .FirstOrDefaultAsync(lp => lp.ListaId == listaId && lp.PistaId == pistaId);

      if (listaPista == null)
      {
        return Json(new { success = false, message = "La pista no está en la lista" });
      }

      _context.ListasPista.Remove(listaPista);

      // Reordenar posiciones
      var pistasRestantes = await _context.ListasPista
          .Where(lp => lp.ListaId == listaId && lp.Posicion > listaPista.Posicion)
          .ToListAsync();

      foreach (var pista in pistasRestantes)
      {
        pista.Posicion--;
      }

      // Actualizar fecha de modificación
      lista.ActualizadoEn = DateTime.UtcNow;

      await _context.SaveChangesAsync();

      return Json(new { success = true, message = "Pista quitada de la lista" });
    }

    /// <summary>
    /// Reordenar pistas en playlist
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReordenarPistas(int listaId, int[] pistasIds)
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return Json(new { success = false, message = "No autenticado" });
      }

      var lista = await _context.ListasReproduccion
          .FirstOrDefaultAsync(l => l.Id == listaId && l.UsuarioId == usuario.Id);

      if (lista == null)
      {
        return Json(new { success = false, message = "Lista no encontrada o no tienes permisos" });
      }

      var listasPista = await _context.ListasPista
          .Where(lp => lp.ListaId == listaId)
          .ToListAsync();

      for (int i = 0; i < pistasIds.Length; i++)
      {
        var listaPista = listasPista.FirstOrDefault(lp => lp.PistaId == pistasIds[i]);
        if (listaPista != null)
        {
          listaPista.Posicion = i + 1;
        }
      }

      lista.ActualizadoEn = DateTime.UtcNow;
      await _context.SaveChangesAsync();

      return Json(new { success = true, message = "Lista reordenada exitosamente" });
    }

    /// <summary>
    /// Seguir/dejar de seguir una playlist
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleSeguir(int listaId)
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return Json(new { success = false, message = "No autenticado" });
      }

      var lista = await _context.ListasReproduccion
          .FirstOrDefaultAsync(l => l.Id == listaId && l.Publica);

      if (lista == null)
      {
        return Json(new { success = false, message = "Lista no encontrada" });
      }

      if (lista.UsuarioId == usuario.Id)
      {
        return Json(new { success = false, message = "No puedes seguir tu propia lista" });
      }

      var siguiendo = await _context.UsuariosSigueLista
          .FirstOrDefaultAsync(usl => usl.UsuarioId == usuario.Id && usl.ListaId == listaId);

      bool estaSiguiendo;
      if (siguiendo != null)
      {
        // Dejar de seguir
        _context.UsuariosSigueLista.Remove(siguiendo);
        estaSiguiendo = false;
      }
      else
      {
        // Comenzar a seguir
        var nuevoSeguimiento = new UsuarioSigueLista
        {
          UsuarioId = usuario.Id,
          ListaId = listaId,
          CreadoEn = DateTime.UtcNow
        };
        _context.UsuariosSigueLista.Add(nuevoSeguimiento);
        estaSiguiendo = true;
      }

      await _context.SaveChangesAsync();

      var mensaje = estaSiguiendo ? "Ahora sigues esta lista" : "Has dejado de seguir esta lista";
      return Json(new { success = true, estaSiguiendo = estaSiguiendo, message = mensaje });
    }
  }
}
