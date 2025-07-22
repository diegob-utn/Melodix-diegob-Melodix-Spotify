using Melodix.Models.Models;

namespace Melodix.Models
{
    // Muchos a muchos: Usuario <-> Artista (seguimiento)
    public class UsuarioSigueArtista
    {
        // Necesarios
        public int Id { get; set; }
        public DateTime CreadoEn { get; set; }

        // FKs
        public string UsuarioId { get; set; }
        public int ArtistaId { get; set; }

        // Navegadores
        public ApplicationUser Usuario { get; set; }
        public Artista Artista { get; set; }
    }
}