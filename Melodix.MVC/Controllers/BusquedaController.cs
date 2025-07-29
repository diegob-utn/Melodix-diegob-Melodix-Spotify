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
  /// Incluye búsqueda en base de datos y archivos locales
  /// </summary>
  [AllowAnonymous]
  public class BusquedaController : Controller
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BusquedaController> _logger;
    private readonly IWebHostEnvironment _environment;

    public BusquedaController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ILogger<BusquedaController> logger,
        IWebHostEnvironment environment)
    {
      _userManager = userManager;
      _context = context;
      _logger = logger;
      _environment = environment;
    }

    public async Task<IActionResult> Index(string? query)
    {
      var viewModel = new BusquedaViewModel { Query = query ?? string.Empty };

      if (!string.IsNullOrWhiteSpace(query))
      {
        var queryLower = query.ToLower();

        // Buscar pistas en base de datos
        viewModel.Pistas = await _context.Pistas
            .Include(p => p.Album)
            .Include(p => p.Usuario)
            .Where(p => p.Titulo.ToLower().Contains(queryLower) ||
                       (p.Album != null && p.Album.Titulo.ToLower().Contains(queryLower)) ||
                       p.Usuario.UserName!.ToLower().Contains(queryLower))
            .OrderByDescending(p => p.CreadoEn)
            .Take(20)
            .ToListAsync();

        // Buscar archivos de música locales
        var archivosLocales = await BuscarArchivosLocales(queryLower);

        // Agregar archivos locales que no estén en la base de datos
        var pistasLocales = new List<Pista>();
        foreach (var archivo in archivosLocales)
        {
          if (!viewModel.Pistas.Any(p => p.RutaArchivo == archivo.RutaArchivo))
          {
            pistasLocales.Add(archivo);
          }
        }

        // Combinar resultados
        viewModel.Pistas = viewModel.Pistas.Concat(pistasLocales).Take(20).ToList();

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
            url = Url.Action("Index", "MusicPlayer") + $"#{p.Id}"
          })
          .ToListAsync();

      // Agregar archivos locales
      var archivosLocales = await BuscarArchivosLocales(termLower);
      var pistasLocales = archivosLocales.Take(3).Select(p => new
      {
        tipo = "archivo_local",
        id = 0,
        titulo = p.Titulo,
        subtitulo = "Archivo Local",
        imagen = "/images/music-default.png",
        url = p.RutaArchivo
      });

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
      sugerencias.AddRange(pistasLocales);
      sugerencias.AddRange(albums);
      sugerencias.AddRange(listas);
      sugerencias.AddRange(usuarios);

      return Json(sugerencias.Take(12));
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
              .Include(p => p.Usuario)
              .Where(p => p.Titulo.ToLower().Contains(queryLower))
              .OrderByDescending(p => p.CreadoEn)
              .Skip((pagina - 1) * cantidad)
              .Take(cantidad)
              .ToListAsync();

          // Agregar archivos locales si es la primera página
          if (pagina == 1)
          {
            var archivosLocales = await BuscarArchivosLocales(queryLower);
            viewModel.Pistas = viewModel.Pistas.Concat(archivosLocales).Take(cantidad).ToList();
          }
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

    #region Métodos Auxiliares

    /// <summary>
    /// Buscar archivos de música en la carpeta local
    /// </summary>
    private async Task<List<Pista>> BuscarArchivosLocales(string query)
    {
      var archivosEncontrados = new List<Pista>();

      try
      {
        var directorioMusica = Path.Combine(_environment.WebRootPath, "uploads", "music");

        if (!Directory.Exists(directorioMusica))
        {
          return archivosEncontrados;
        }

        var extensionesAudio = new[] { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".m4a" };

        var archivos = Directory.GetFiles(directorioMusica, "*.*", SearchOption.TopDirectoryOnly)
            .Where(archivo => extensionesAudio.Contains(Path.GetExtension(archivo).ToLowerInvariant()))
            .Where(archivo => string.IsNullOrEmpty(query) ||
                            Path.GetFileNameWithoutExtension(archivo).ToLower().Contains(query))
            .Take(20);

        foreach (var rutaArchivo in archivos)
        {
          var nombreArchivo = Path.GetFileNameWithoutExtension(rutaArchivo);
          var extension = Path.GetExtension(rutaArchivo);
          var rutaRelativa = $"/uploads/music/{Path.GetFileName(rutaArchivo)}";

          // Verificar si ya existe en la base de datos
          var pistaExistente = await _context.Pistas
              .FirstOrDefaultAsync(p => p.RutaArchivo == rutaRelativa);

          if (pistaExistente == null)
          {
            // Crear pista temporal para mostrar el archivo local
            var pistaLocal = new Pista
            {
              Id = 0, // ID temporal para archivos locales
              Titulo = LimpiarNombreArchivo(nombreArchivo),
              RutaArchivo = rutaRelativa,
              Genero = GeneroMusica.Desconocido,
              FechaSubida = System.IO.File.GetCreationTime(rutaArchivo),
              CreadoEn = System.IO.File.GetCreationTime(rutaArchivo),
              ActualizadoEn = System.IO.File.GetLastWriteTime(rutaArchivo),
              ContadorReproducciones = 0,
              EsExplicita = false,
              UsuarioId = "local", // Identificador para archivos locales
              Usuario = new ApplicationUser { UserName = "Archivo Local", Id = "local" },
              Album = new Album { Titulo = "Archivos Locales", Id = 0 }
            };

            archivosEncontrados.Add(pistaLocal);
          }
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al buscar archivos locales con query: {Query}", query);
      }

      return archivosEncontrados;
    }

    /// <summary>
    /// Limpiar nombre de archivo para mostrarlo como título
    /// </summary>
    private string LimpiarNombreArchivo(string nombreArchivo)
    {
      // Remover GUID si está presente
      var partes = nombreArchivo.Split('-');
      if (partes.Length > 1 && Guid.TryParse(partes[0], out _))
      {
        return string.Join("-", partes.Skip(1));
      }

      // Reemplazar guiones y guiones bajos con espacios
      return nombreArchivo.Replace("-", " ").Replace("_", " ");
    }

    /// <summary>
    /// Obtener todos los archivos de música locales
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ArchivosLocales()
    {
      var archivosLocales = await BuscarArchivosLocales("");

      var viewModel = new BusquedaViewModel
      {
        Query = "Archivos Locales",
        Pistas = archivosLocales
      };

      return View("Index", viewModel);
    }

    #endregion
  }
}
