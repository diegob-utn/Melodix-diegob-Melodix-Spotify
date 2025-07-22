using Melodix.Models.Models;

namespace Melodix.Models
{
    public class HistorialEscucha
    {
        // Necesarios
        public int Id { get; set; }
        public DateTime EscuchadaEn { get; set; }

        // FKs
        public string UsuarioId { get; set; }
        public int PistaId { get; set; }

        // Navegadores
        public ApplicationUser Usuario { get; set; }
        public Pista Pista { get; set; }
    }
}