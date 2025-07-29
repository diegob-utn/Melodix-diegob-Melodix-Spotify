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
    /// Controlador para funciones administrativas del sistema
    /// Modelos principales: ApplicationUser, Pista, Album, ListaReproduccion, Genero, PlanSuscripcion
    /// </summary>
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        /// <summary>
        /// Dashboard principal del administrador
        /// </summary>
        public async Task<IActionResult> Dashboard()
        {
            var estadisticas = new AdminDashboardViewModel
            {
                TotalUsuarios = await _context.Users.CountAsync(),
                TotalPistas = await _context.Pistas.CountAsync(),
                TotalAlbumes = await _context.Albums.CountAsync(),
                TotalListas = await _context.ListasReproduccion.CountAsync(),
                TotalGeneros = await _context.Generos.CountAsync(),
                UsuariosActivos = await _context.Users
                    .Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTime.UtcNow)
                    .CountAsync(),
                ReproduccionesHoy = await _context.HistorialesEscucha
                    .Where(h => h.EscuchadaEn.Date == DateTime.UtcNow.Date)
                    .CountAsync(),
                SuscripcionesActivas = await _context.Suscripciones
                    .CountAsync(s => s.Estado == EstadoSuscripcion.Activa)
            };

            // Últimos usuarios registrados
            estadisticas.UltimosUsuarios = await _context.Users
                .OrderByDescending(u => u.CreadoEn)
                .Take(5)
                .Select(u => new UsuarioResumeViewModel
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Email = u.Email ?? "",
                    Nick = u.Nick,
                    FechaRegistro = u.CreadoEn
                })
                .ToListAsync();

            // Pistas más populares
            estadisticas.PistasMasPopulares = await _context.Pistas
                .OrderByDescending(p => p.ContadorReproducciones)
                .Take(10)
                .ToListAsync();

            return View(estadisticas);
        }

        /// <summary>
        /// Gestión de usuarios
        /// </summary>
        public async Task<IActionResult> Usuarios(int pagina = 1, string? busqueda = null)
        {
            const int tamañoPagina = 20;

            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(u => u.Nombre.Contains(busqueda) ||
                                        u.Nick.Contains(busqueda) ||
                                        (u.Email != null && u.Email.Contains(busqueda)));
            }

            var totalUsuarios = await query.CountAsync();
            var usuarios = await query
                .OrderByDescending(u => u.CreadoEn)
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .ToListAsync();

            var viewModel = new AdminUsuariosViewModel
            {
                Usuarios = usuarios.Select(u => new UsuarioResumeViewModel
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Email = u.Email ?? "",
                    Nick = u.Nick,
                    FechaRegistro = u.CreadoEn,
                    UltimoAcceso = null, // TODO: Implementar seguimiento de último acceso
                    EstaBloqueado = u.LockoutEnd.HasValue && u.LockoutEnd > DateTime.UtcNow
                }).ToList(),
                PaginaActual = pagina,
                TotalPaginas = (int)Math.Ceiling(totalUsuarios / (double)tamañoPagina),
                Busqueda = busqueda,
                TotalUsuarios = totalUsuarios
            };

            return View(viewModel);
        }

        /// <summary>
        /// Ver detalles de un usuario
        /// </summary>
        public async Task<IActionResult> DetalleUsuario(string id)
        {
            var usuario = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(usuario);

            var viewModel = new AdminDetalleUsuarioViewModel
            {
                Usuario = usuario,
                Roles = roles.ToList(),
                TotalPistas = await _context.Pistas.CountAsync(p => p.UsuarioId == id),
                TotalAlbumes = await _context.Albums.CountAsync(a => a.UsuarioId == id),
                TotalListas = await _context.ListasReproduccion.CountAsync(l => l.UsuarioId == id),
                TotalReproducciones = await _context.HistorialesEscucha
                    .Include(h => h.Pista)
                    .Where(h => h.Pista.UsuarioId == id)
                    .CountAsync(),
                SuscripcionActual = await _context.Suscripciones
                    .Include(s => s.Plan)
                    .FirstOrDefaultAsync(s => s.UsuarioId == id && s.Estado == EstadoSuscripcion.Activa)
            };

            return View(viewModel);
        }

        /// <summary>
        /// Bloquear/desbloquear usuario
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BloquearUsuario(string id, bool bloquear)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado" });
            }

            try
            {
                if (bloquear)
                {
                    // Bloquear por 100 años (permanente)
                    await _userManager.SetLockoutEndDateAsync(usuario, DateTimeOffset.UtcNow.AddYears(100));
                    await _userManager.SetLockoutEnabledAsync(usuario, true);
                }
                else
                {
                    // Desbloquear
                    await _userManager.SetLockoutEndDateAsync(usuario, null);
                }

                var mensaje = bloquear ? "Usuario bloqueado exitosamente" : "Usuario desbloqueado exitosamente";
                return Json(new { success = true, message = mensaje });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al {Action} usuario {UserId}", bloquear ? "bloquear" : "desbloquear", id);
                return Json(new { success = false, message = "Error al procesar la solicitud" });
            }
        }

        /// <summary>
        /// Cambiar rol de usuario
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarRol(string userId, string nuevoRol)
        {
            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado" });
            }

            if (!await _roleManager.RoleExistsAsync(nuevoRol))
            {
                return Json(new { success = false, message = "Rol no válido" });
            }

            try
            {
                // Remover todos los roles actuales
                var rolesActuales = await _userManager.GetRolesAsync(usuario);
                await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);

                // Agregar nuevo rol
                await _userManager.AddToRoleAsync(usuario, nuevoRol);

                return Json(new { success = true, message = $"Rol cambiado a {nuevoRol} exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar rol del usuario {UserId} a {NuevoRol}", userId, nuevoRol);
                return Json(new { success = false, message = "Error al cambiar el rol" });
            }
        }

        /// <summary>
        /// Gestión de contenido (pistas y álbumes)
        /// </summary>
        public async Task<IActionResult> Contenido(int pagina = 1, string? busqueda = null, string? tipo = null)
        {
            const int tamañoPagina = 20;

            var viewModel = new AdminContenidoViewModel
            {
                PaginaActual = pagina,
                Busqueda = busqueda,
                TipoFiltro = tipo
            };

            if (tipo == "albums" || string.IsNullOrEmpty(tipo))
            {
                var queryAlbumes = _context.Albums
                    .Include(a => a.Usuario)
                    .Include(a => a.Pistas)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(busqueda))
                {
                    queryAlbumes = queryAlbumes.Where(a => a.Titulo.Contains(busqueda) ||
                                                          a.Usuario.Nombre.Contains(busqueda));
                }

                var totalAlbumes = await queryAlbumes.CountAsync();
                viewModel.Albums = await queryAlbumes
                    .OrderByDescending(a => a.FechaLanzamiento)
                    .Skip((pagina - 1) * tamañoPagina)
                    .Take(tamañoPagina)
                    .ToListAsync();

                viewModel.TotalPaginas = (int)Math.Ceiling(totalAlbumes / (double)tamañoPagina);
            }

            if (tipo == "pistas" || string.IsNullOrEmpty(tipo))
            {
                var queryPistas = _context.Pistas
                    .Include(p => p.Usuario)
                    .Include(p => p.Album)
                    .Include(p => p.Genero)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(busqueda))
                {
                    queryPistas = queryPistas.Where(p => p.Titulo.Contains(busqueda) ||
                                                        p.Usuario.Nombre.Contains(busqueda));
                }

                var totalPistas = await queryPistas.CountAsync();
                viewModel.Pistas = await queryPistas
                    .OrderByDescending(p => p.FechaSubida)
                    .Skip((pagina - 1) * tamañoPagina)
                    .Take(tamañoPagina)
                    .ToListAsync();

                if (string.IsNullOrEmpty(tipo))
                {
                    viewModel.TotalPaginas = Math.Max(viewModel.TotalPaginas,
                        (int)Math.Ceiling(totalPistas / (double)tamañoPagina));
                }
                else
                {
                    viewModel.TotalPaginas = (int)Math.Ceiling(totalPistas / (double)tamañoPagina);
                }
            }

            return View(viewModel);
        }

        /// <summary>
        /// Eliminar pista
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarPista(int id)
        {
            var pista = await _context.Pistas.FindAsync(id);
            if (pista == null)
            {
                return Json(new { success = false, message = "Pista no encontrada" });
            }

            try
            {
                // Eliminar registros relacionados
                var listaPistas = await _context.ListasPista
                    .Where(lp => lp.PistaId == id)
                    .ToListAsync();
                _context.ListasPista.RemoveRange(listaPistas);

                var historialEscucha = await _context.HistorialesEscucha
                    .Where(h => h.PistaId == id)
                    .ToListAsync();
                _context.HistorialesEscucha.RemoveRange(historialEscucha);

                var likes = await _context.UsuariosLikePista
                    .Where(ulp => ulp.PistaId == id)
                    .ToListAsync();
                _context.UsuariosLikePista.RemoveRange(likes);

                // Eliminar la pista
                _context.Pistas.Remove(pista);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Pista {PistaId} eliminada por administrador", id);
                return Json(new { success = true, message = "Pista eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar pista {PistaId}", id);
                return Json(new { success = false, message = "Error al eliminar la pista" });
            }
        }

        /// <summary>
        /// Eliminar álbum
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarAlbum(int id)
        {
            var album = await _context.Albums
                .Include(a => a.Pistas)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (album == null)
            {
                return Json(new { success = false, message = "Álbum no encontrado" });
            }

            try
            {
                // Eliminar todas las pistas del álbum
                foreach (var pista in album.Pistas)
                {
                    var listaPistas = await _context.ListasPista
                        .Where(lp => lp.PistaId == pista.Id)
                        .ToListAsync();
                    _context.ListasPista.RemoveRange(listaPistas);

                    var historialEscucha = await _context.HistorialesEscucha
                        .Where(h => h.PistaId == pista.Id)
                        .ToListAsync();
                    _context.HistorialesEscucha.RemoveRange(historialEscucha);

                    var likes = await _context.UsuariosLikePista
                        .Where(ulp => ulp.PistaId == pista.Id)
                        .ToListAsync();
                    _context.UsuariosLikePista.RemoveRange(likes);
                }

                // Eliminar likes del álbum
                var albumLikes = await _context.UsuariosLikeAlbum
                    .Where(ula => ula.AlbumId == id)
                    .ToListAsync();
                _context.UsuariosLikeAlbum.RemoveRange(albumLikes);

                // Eliminar el álbum (las pistas se eliminan en cascada)
                _context.Albums.Remove(album);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Álbum {AlbumId} eliminado por administrador", id);
                return Json(new { success = true, message = "Álbum eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar álbum {AlbumId}", id);
                return Json(new { success = false, message = "Error al eliminar el álbum" });
            }
        }

        /// <summary>
        /// Gestión de géneros musicales
        /// </summary>
        public async Task<IActionResult> Generos()
        {
            var generos = await _context.Generos
                .Select(g => new GeneroViewModel
                {
                    Id = g.Id,
                    Nombre = g.Nombre,
                    Descripcion = g.Descripcion,
                    TotalPistas = g.Pistas.Count()
                })
                .OrderBy(g => g.Nombre)
                .ToListAsync();

            return View(generos);
        }

        /// <summary>
        /// Crear nuevo género
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearGenero(CrearGeneroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Datos inválidos" });
            }

            // Verificar que no exista un género con el mismo nombre
            var generoExistente = await _context.Generos
                .AnyAsync(g => g.Nombre.ToLower() == model.Nombre.ToLower());

            if (generoExistente)
            {
                return Json(new { success = false, message = "Ya existe un género con ese nombre" });
            }

            try
            {
                var nuevoGenero = new Genero
                {
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion ?? string.Empty
                };

                _context.Generos.Add(nuevoGenero);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Género creado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear género {Nombre}", model.Nombre);
                return Json(new { success = false, message = "Error al crear el género" });
            }
        }

        /// <summary>
        /// Eliminar género
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarGenero(int id)
        {
            var genero = await _context.Generos
                .Include(g => g.Pistas)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (genero == null)
            {
                return Json(new { success = false, message = "Género no encontrado" });
            }

            if (genero.Pistas.Any())
            {
                return Json(new { success = false, message = "No se puede eliminar un género que tiene pistas asociadas" });
            }

            try
            {
                _context.Generos.Remove(genero);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Género eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar género {GeneroId}", id);
                return Json(new { success = false, message = "Error al eliminar el género" });
            }
        }

        /// <summary>
        /// Reportes y estadísticas avanzadas
        /// </summary>
        public async Task<IActionResult> Reportes()
        {
            var reportes = new AdminReportesViewModel();

            // Estadísticas de usuarios por mes (último año)
            var fechaInicio = DateTime.UtcNow.AddMonths(-12);
            reportes.UsuariosPorMes = await _context.Users
                .Where(u => u.CreadoEn.HasValue && u.CreadoEn >= fechaInicio)
                .GroupBy(u => new { u.CreadoEn!.Value.Year, u.CreadoEn!.Value.Month })
                .Select(g => new EstadisticaMensualViewModel
                {
                    Año = g.Key.Year,
                    Mes = g.Key.Month,
                    Cantidad = g.Count()
                })
                .OrderBy(e => e.Año).ThenBy(e => e.Mes)
                .ToListAsync();

            // Reproducciones por mes
            reportes.ReproduccionesPorMes = await _context.HistorialesEscucha
                .Where(h => h.EscuchadaEn >= fechaInicio)
                .GroupBy(h => new { h.EscuchadaEn.Year, h.EscuchadaEn.Month })
                .Select(g => new EstadisticaMensualViewModel
                {
                    Año = g.Key.Year,
                    Mes = g.Key.Month,
                    Cantidad = g.Count()
                })
                .OrderBy(e => e.Año).ThenBy(e => e.Mes)
                .ToListAsync();

            // Géneros más populares
            reportes.GenerosMasPopulares = await _context.Generos
                .Select(g => new GeneroPopularidadViewModel
                {
                    Nombre = g.Nombre,
                    TotalPistas = g.Pistas.Count(),
                    TotalReproducciones = g.Pistas.Sum(p => p.ContadorReproducciones)
                })
                .OrderByDescending(g => g.TotalReproducciones)
                .Take(10)
                .ToListAsync();

            return View(reportes);
        }
    }
}
