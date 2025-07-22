using Melodix.Models.Models;

namespace Melodix.Models
{
    // Muchos a muchos: Suscripcion <-> Usuario
    public class SuscripcionUsuario
    {
        // Necesarios
        public int Id { get; set; }

        // FKs
        public int SuscripcionId { get; set; }
        public string UsuarioId { get; set; }

        // Navegadores
        public Suscripcion Suscripcion { get; set; }
        public ApplicationUser Usuario { get; set; }
    }
}