using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Melodix.Models.Models
{

    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nick { get; set; }

        [MaxLength(255)]
        public string? FotoPerfil { get; set; }

        [MaxLength(255)]
        public string? Biografia { get; set; }

        [MaxLength(100)]
        public string? Ubicacion { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        public GeneroUsuario? Genero { get; set; } // Enum

        public RolUsuario? Rol { get; set; }
        public bool Activo { get; set; }
        public bool Verificado { get; set; }

        public DateTime? CreadoEn { get; set; }
        public DateTime? ActualizadoEn { get; set; }

        public string? SpotifyId { get; set; }
        public string? SpotifyAccountType { get; set; }

        public string? SpotifyAccessToken { get; set; }
        public string? SpotifyRefreshToken { get; set; }
        public DateTime? SpotifyTokenExpiration { get; set; }


        public string? Proveedor { get; set; }

        // Navegadores
        public List<ListaReproduccion> ListasReproduccion { get; set; } = new();
        public List<HistorialEscucha> HistorialEscuchas { get; set; } = new();
        public List<UsuarioSigue> Seguidos { get; set; } = new();
        public List<UsuarioSigue> Seguidores { get; set; } = new();
        public List<UsuarioLikeAlbum> UsuarioLikeAlbums { get; set; } = new();
        public List<UsuarioLikeLista> UsuarioLikeListas { get; set; } = new();
        public List<UsuarioLikePista> UsuarioLikePistas { get; set; } = new();
        public List<UsuarioSigueArtista> UsuarioSigueArtistas { get; set; } = new();
        public List<UsuarioSigueLista> UsuarioSigueListas { get; set; } = new();
        public List<HistorialLike> HistorialLikes { get; set; } = new();
        public List<SuscripcionUsuario> SuscripcionUsuarios { get; set; } = new();
        public List<Suscripcion> Suscripciones { get; set; } = new();
        public List<ArchivoSubido> ArchivosSubidos { get; set; } = new();
        public List<TransaccionPago> TransaccionesPago { get; set; } = new();
        public List<LogSistema> LogsGenerados { get; set; } = new();
        public List<SolicitudMusico> SolicitudesMusico { get; set; } = new();
        public List<SolicitudMusico> SolicitudesRevisadas { get; set; } = new();
    }
}
