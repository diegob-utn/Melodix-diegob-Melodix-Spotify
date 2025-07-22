using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melodix.Models.Models
{
    // 4. Solicitud de ascenso a Músico
    public class SolicitudMusico
    {
        // Necesarios
        public int Id { get; set; }
        public string UsuarioId { get; set; }
        public string? Mensaje { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public EstadoSolicitudMusico Estado { get; set; }
        public DateTime? FechaRevision { get; set; }
        public string? AdminRevisorId { get; set; }
        public string? MotivoRechazo { get; set; }

        // Navegadores
        public ApplicationUser Usuario { get; set; }
        public ApplicationUser? AdminRevisor { get; set; }
        }
}
