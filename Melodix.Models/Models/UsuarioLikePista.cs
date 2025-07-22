using Melodix.Models.Models;

namespace Melodix.Models
{
    // Muchos a muchos: Usuario <-> Pista (likes)
    public class UsuarioLikePista
    {
        // Necesarios
        public int Id { get; set; }
        public DateTime CreadoEn { get; set; }

        // FKs
        public string UsuarioId { get; set; }
        public int PistaId { get; set; }

        // Navegadores
        public ApplicationUser Usuario { get; set; }
        public Pista Pista { get; set; }
    }
}