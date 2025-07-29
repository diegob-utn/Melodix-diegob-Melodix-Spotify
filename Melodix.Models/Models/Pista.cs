using Melodix.Models.Models;
using System.ComponentModel.DataAnnotations;

namespace Melodix.Models
{
    public class Pista
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        public int Duracion { get; set; }
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }

        public DateTime? FechaLanzamiento { get; set; }
        public DateTime FechaSubida { get; set; }

        [MaxLength(255)]
        public string? UrlPortada { get; set; }

        [MaxLength(255)]
        public string? RutaArchivo { get; set; }

        [MaxLength(255)]
        public string? RutaImagen { get; set; }

        [MaxLength(100)]
        public string? SpotifyPistaId { get; set; }

        public int AlbumId { get; set; }
        public virtual Album Album { get; set; } = null!;

        // Usuario que subió la pista (artista)
        [Required]
        public string UsuarioId { get; set; } = string.Empty;
        public virtual ApplicationUser Usuario { get; set; } = null!;

        public int? GeneroId { get; set; }
        public virtual Genero? Genero { get; set; }

        public bool EsExplicita { get; set; }
        public int ContadorReproducciones { get; set; }

        public virtual List<ListaPista> ListaPistas { get; set; } = new();
        public virtual List<HistorialEscucha> HistorialEscuchas { get; set; } = new();
        public virtual List<UsuarioLikePista> UsuarioLikePistas { get; set; } = new();
        public virtual List<ArchivoSubido> ArchivosMusica { get; set; } = new();
    }
}