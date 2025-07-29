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
  /// Controlador para ver planes, contratar, simular pago, ver suscripciones activas
  /// Modelos principales: PlanSuscripcion, Suscripcion, SuscripcionUsuario, TransaccionPago
  /// </summary>
  [Authorize]
  public class SuscripcionController : Controller
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SuscripcionController> _logger;

    public SuscripcionController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ILogger<SuscripcionController> logger)
    {
      _userManager = userManager;
      _context = context;
      _logger = logger;
    }

    /// <summary>
    /// Ver todos los planes disponibles
    /// </summary>
    public async Task<IActionResult> Planes()
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return RedirectToAction("Login", "Cuenta");
      }

      var planes = await _context.PlanesSuscripcion
          .OrderBy(p => p.Precio)
          .ToListAsync();

      var suscripcionActual = await _context.Suscripciones
          .Include(s => s.Plan)
          .FirstOrDefaultAsync(s => s.UsuarioId == usuario.Id &&
                                  s.Estado == EstadoSuscripcion.Activa);

      var viewModel = new SuscripcionViewModel
      {
        Planes = planes,
        SuscripcionActual = suscripcionActual,
        Usuario = usuario,
        TieneSuscripcionActiva = suscripcionActual != null
      };

      return View(viewModel);
    }

    /// <summary>
    /// Ver mi plan actual
    /// </summary>
    public async Task<IActionResult> MiPlan()
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return RedirectToAction("Login", "Cuenta");
      }

      var suscripcion = await _context.Suscripciones
          .Include(s => s.Plan)
          .Include(s => s.SuscripcionUsuarios)
          .FirstOrDefaultAsync(s => s.UsuarioId == usuario.Id &&
                                  s.Estado == EstadoSuscripcion.Activa);

      var historialTransacciones = await _context.TransaccionesPago
          .Where(t => t.UsuarioId == usuario.Id)
          .OrderByDescending(t => t.Fecha)
          .Take(10)
          .ToListAsync();

      ViewBag.HistorialTransacciones = historialTransacciones;

      return View(suscripcion);
    }

    /// <summary>
    /// Página para comprar un plan
    /// </summary>
    public async Task<IActionResult> Comprar(int planId)
    {
      var plan = await _context.PlanesSuscripcion.FindAsync(planId);
      if (plan == null)
      {
        return NotFound("Plan no encontrado");
      }

      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return RedirectToAction("Login", "Cuenta");
      }

      // Verificar si ya tiene una suscripción activa
      var suscripcionActiva = await _context.Suscripciones
          .AnyAsync(s => s.UsuarioId == usuario.Id && s.Estado == EstadoSuscripcion.Activa);

      if (suscripcionActiva)
      {
        TempData["Warning"] = "Ya tienes una suscripción activa. Primero debes cancelarla.";
        return RedirectToAction("MiPlan");
      }

      ViewBag.Plan = plan;
      ViewBag.Usuario = usuario;

      return View();
    }

    /// <summary>
    /// Confirmar pago y activar suscripción (simulado)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarPago(int planId, string metodoPago = "tarjeta")
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return Json(new { success = false, message = "No autenticado" });
      }

      var plan = await _context.PlanesSuscripcion.FindAsync(planId);
      if (plan == null)
      {
        return Json(new { success = false, message = "Plan no encontrado" });
      }

      // Verificar suscripción activa
      var suscripcionActiva = await _context.Suscripciones
          .AnyAsync(s => s.UsuarioId == usuario.Id && s.Estado == EstadoSuscripcion.Activa);

      if (suscripcionActiva)
      {
        return Json(new { success = false, message = "Ya tienes una suscripción activa" });
      }

      using var transaction = await _context.Database.BeginTransactionAsync();

      try
      {
        // Crear nueva suscripción
        var nuevaSuscripcion = new Suscripcion
        {
          UsuarioId = usuario.Id,
          PlanId = planId,
          FechaInicio = DateTime.UtcNow,
          FechaFin = DateTime.UtcNow.AddMonths(plan.DuracionMeses),
          Estado = EstadoSuscripcion.Activa
        };

        _context.Suscripciones.Add(nuevaSuscripcion);
        await _context.SaveChangesAsync();

        // Crear relación usuario-suscripción
        var suscripcionUsuario = new SuscripcionUsuario
        {
          UsuarioId = usuario.Id,
          SuscripcionId = nuevaSuscripcion.Id
        };

        _context.SuscripcionesUsuario.Add(suscripcionUsuario);

        // Registrar transacción de pago (simulada)
        var transaccion = new TransaccionPago
        {
          UsuarioId = usuario.Id,
          SuscripcionId = nuevaSuscripcion.Id,
          Monto = plan.Precio,
          Fecha = DateTime.UtcNow,
          Estado = EstadoPago.Exitoso,
          Servicio = ServicioPago.Simulado,
          ReferenciaExterna = $"SIM_{DateTime.UtcNow:yyyyMMddHHmmss}",
          Detalle = $"Pago simulado para plan {plan.Nombre}",
          JsonRespuesta = "{\"status\":\"success\",\"method\":\"" + metodoPago + "\"}"
        };

        _context.TransaccionesPago.Add(transaccion);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return Json(new
        {
          success = true,
          message = "Suscripción activada exitosamente",
          redirectUrl = Url.Action("MiPlan")
        });
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error al procesar pago para usuario {UserId}", usuario.Id);

        return Json(new
        {
          success = false,
          message = "Error al procesar el pago. Inténtalo de nuevo."
        });
      }
    }

    /// <summary>
    /// Cancelar suscripción actual
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar()
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return Json(new { success = false, message = "No autenticado" });
      }

      var suscripcion = await _context.Suscripciones
          .FirstOrDefaultAsync(s => s.UsuarioId == usuario.Id &&
                                  s.Estado == EstadoSuscripcion.Activa);

      if (suscripcion == null)
      {
        return Json(new { success = false, message = "No tienes una suscripción activa" });
      }

      suscripcion.Estado = EstadoSuscripcion.Cancelada;
      // Nota: La fecha de fin se mantiene, así el usuario puede seguir usando 
      // el servicio hasta que expire

      await _context.SaveChangesAsync();

      return Json(new
      {
        success = true,
        message = "Suscripción cancelada. Podrás usar el servicio hasta " +
                   suscripcion.FechaFin?.ToString("dd/MM/yyyy")
      });
    }

    /// <summary>
    /// Reactivar suscripción cancelada (si no ha expirado)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reactivar()
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return Json(new { success = false, message = "No autenticado" });
      }

      var suscripcion = await _context.Suscripciones
          .FirstOrDefaultAsync(s => s.UsuarioId == usuario.Id &&
                                  s.Estado == EstadoSuscripcion.Cancelada &&
                                  s.FechaFin > DateTime.UtcNow);

      if (suscripcion == null)
      {
        return Json(new
        {
          success = false,
          message = "No tienes una suscripción cancelada válida para reactivar"
        });
      }

      suscripcion.Estado = EstadoSuscripcion.Activa;
      await _context.SaveChangesAsync();

      return Json(new
      {
        success = true,
        message = "Suscripción reactivada exitosamente"
      });
    }

    /// <summary>
    /// Verificar si el usuario tiene acceso premium
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> VerificarAccesoPremium()
    {
      var usuario = await _userManager.GetUserAsync(User);
      if (usuario == null)
      {
        return Json(new { tienePremium = false });
      }

      var tieneSuscripcionActiva = await _context.Suscripciones
          .AnyAsync(s => s.UsuarioId == usuario.Id &&
                       s.Estado == EstadoSuscripcion.Activa &&
                       s.FechaFin > DateTime.UtcNow);

      return Json(new { tienePremium = tieneSuscripcionActiva });
    }
  }
}
