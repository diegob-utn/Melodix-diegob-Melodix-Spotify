
using Melodix.Models.Models;

namespace Melodix.Models
{
    public class Suscripcion
    {
        // Necesarios
        public int Id { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public EstadoSuscripcion Estado { get; set; }

        // FKs
        public string UsuarioId { get; set; } // Titular de la suscripción
        public int PlanId { get; set; }

        // Navegadores
        public ApplicationUser Usuario { get; set; }
        public PlanSuscripcion Plan { get; set; }
        public List<SuscripcionUsuario> SuscripcionUsuarios { get; set; } = new();
        public List<TransaccionPago> TransaccionesPago { get; set; } = new();
    }
}