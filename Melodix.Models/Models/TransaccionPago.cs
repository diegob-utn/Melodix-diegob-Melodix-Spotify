using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melodix.Models.Models
{
    // 1. Pagos/Transacciones
    public class TransaccionPago
    {
        // Necesarios
        public int Id { get; set; }
        public string UsuarioId { get; set; }
        public int? SuscripcionId { get; set; } // Opcional: pago de suscripción
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public EstadoPago Estado { get; set; }
        public ServicioPago Servicio { get; set; } // Simulado, Peigo, etc.
        public string? ReferenciaExterna { get; set; }
        public string? Detalle { get; set; }
        public string? JsonRespuesta { get; set; }

        // Navegadores
        public ApplicationUser Usuario { get; set; }
        public Suscripcion? Suscripcion { get; set; }
    }

}
