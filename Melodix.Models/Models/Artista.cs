using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Melodix.Models
{
    /// <summary>
    /// Representa un artista musical.
    /// </summary>
    public class Artista
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; }

        [MaxLength(100)]
        public string SpotifyArtistaId { get; set; }

        [MaxLength(512)]
        public string UrlImagen { get; set; }

        // Navegación
        public virtual List<Pista> Pistas { get; set; } = new();
        public virtual List<Album> Albumes { get; set; } = new();
        public virtual List<UsuarioSigueArtista> UsuariosQueSiguen { get; set; } = new();
    }
}