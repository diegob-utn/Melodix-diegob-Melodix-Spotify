using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melodix.Models.Models
{
    // 3. Logs / Auditoría / Errores (simple)
    public class LogSistema
    {
        // Necesarios
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public NivelLog Nivel { get; set; } // Info, Warning, Error, Fatal
        public string Mensaje { get; set; }
        public string? Detalle { get; set; }
        public string? StackTrace { get; set; }
        public string? UsuarioId { get; set; } // Opcional: usuario que lo generó

        // Navegadores
        public ApplicationUser? Usuario { get; set; }
    }
}
