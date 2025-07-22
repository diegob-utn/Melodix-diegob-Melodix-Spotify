using Melodix.Models.Models;

namespace Melodix.Models
{
    // Muchos a muchos: Usuario <-> Album (likes)
    public class UsuarioLikeAlbum
    {
        // Necesarios
        public int Id { get; set; }
        public DateTime CreadoEn { get; set; }

        // FKs
        public string UsuarioId { get; set; }
        public int AlbumId { get; set; }

        // Navegadores
        public ApplicationUser Usuario { get; set; }
        public Album Album { get; set; }
    }
}