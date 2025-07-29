using System.ComponentModel.DataAnnotations;
using Melodix.Models.Models;

namespace Melodix.Models
{
    public class Album
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        public DateTime? FechaLanzamiento { get; set; }

        [MaxLength(255)]
        public string? UrlPortada { get; set; }

        [MaxLength(255)]
        public string? RutaImagen { get; set; }

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [MaxLength(100)]
        public string? SpotifyAlbumId { get; set; }

        // Usuario que creó el álbum (artista)
        [Required]
        public string UsuarioId { get; set; } = string.Empty;
        public virtual ApplicationUser Usuario { get; set; } = null!;

        public virtual List<Pista> Pistas { get; set; } = new();
        public virtual List<UsuarioLikeAlbum> UsuarioLikeAlbums { get; set; } = new();
    }
}