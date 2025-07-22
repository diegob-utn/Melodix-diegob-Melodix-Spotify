using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melodix.Models
{
  // ================================
  // ENUMS
  // ================================

  // Enums
  public enum RolUsuario
  {
    Usuario = 0,
    Musico = 1,
    Admin = 2
  }

  public enum TipoObjetoLike
  {
    Pista = 0,
    Album = 1,
    Lista = 2
  }

  public enum AccionLike
  {
    Like = 0,
    Unlike = 1
  }

  public enum EstadoSuscripcion
  {
    Activa = 0,
    Cancelada = 1,
    Pendiente = 2,
    Vencida = 3
  }

  public enum EstadoPago
  {
    Pendiente = 0,
    Exitoso = 1,
    Fallido = 2,
    Cancelado = 3
  }

  public enum ServicioPago
  {
    Simulado = 0,
    Peigo = 1 /* Agrega otros servicios reales a futuro */
  }

  public enum TipoArchivo
  {
    Musica = 0,
    Imagen = 1,
    Otro = 2
  }

  public enum NivelLog
  {
    Info = 0,
    Warning = 1,
    Error = 2,
    Fatal = 3
  }

  public enum EstadoSolicitudMusico
  {
    Pendiente = 0,
    Aprobada = 1,
    Rechazada = 2
  }

  public enum GeneroUsuario
  {
    Hombre = 0,
    Mujer = 1
  }

  // ================================
  // CORE MODELS
  // ================================

  public class Album
  {
    // Necesarios
    public int Id { get; set; }
    public string Titulo { get; set; }
    public DateTime? FechaLanzamiento { get; set; }
    public string UrlPortada { get; set; }
    public string SpotifyAlbumId { get; set; }

    // FKs
    public int ArtistaId { get; set; }

    // Navegadores
    public Artista Artista { get; set; }
    public List<Pista> Pistas { get; set; } = new();
    public List<UsuarioLikeAlbum> UsuarioLikeAlbums { get; set; } = new();
  }

  // Usuario Identity + perfil extendido
  public class ApplicationUser : IdentityUser
  {
    [Required(ErrorMessage = "El campo Nombre es obligatorio.")]
    [MaxLength(100)]
    public string Nombre { get; set; }

    [Required(ErrorMessage = "El campo Nick es obligatorio.")]
    [MaxLength(50)]
    public string Nick { get; set; }

    [MaxLength(255)]
    public string? FotoPerfil { get; set; }

    [MaxLength(255)]
    public string? Biografia { get; set; }

    [MaxLength(100)]
    public string? Ubicacion { get; set; }

    public DateTime? FechaNacimiento { get; set; }

    public GeneroUsuario? Genero { get; set; } // Nuevo campo

    public RolUsuario? Rol { get; set; }
    public bool? Activo { get; set; }
    public bool Verificado { get; set; }

    public DateTime? CreadoEn { get; set; }
    public DateTime? ActualizadoEn { get; set; }

    public string? SpotifyId { get; set; }
    public string? SpotifyAccessToken { get; set; }
    public string? SpotifyRefreshToken { get; set; }
    public string? SpotifyAccountType { get; set; }

    public string? Proveedor { get; set; } // Google, Facebook, etc.

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

  // 2. Archivos subidos (música, imágenes, otros)
  public class ArchivoSubido
  {
    // Necesarios
    [Key]
    public int Id { get; set; }

    [Required]
    public string UsuarioId { get; set; }

    [Required]
    public TipoArchivo Tipo { get; set; }

    [Required, MaxLength(255)]
    public string NombreArchivo { get; set; }

    [Required, MaxLength(1024)]
    public string RutaAlmacenamiento { get; set; }

    [Required]
    public long TamanoBytes { get; set; }

    [Required]
    public DateTime FechaSubida { get; set; }

    [Required]
    public bool EsPublico { get; set; }

    [MaxLength(12)]
    public string? Extension { get; set; }

    [MaxLength(128)]
    public string? HashArchivo { get; set; } // UNIQUE si querés evitar duplicados

    [MaxLength(512)]
    public string? Descripcion { get; set; }

    [MaxLength(64)]
    public string? EntidadReferencia { get; set; } // Ej: "Pista", "ApplicationUser"

    public int? IdReferencia { get; set; } // ID de la pista o entidad

    // Navegadores
    [ForeignKey(nameof(UsuarioId))]
    public virtual ApplicationUser Usuario { get; set; }
  }

  // Música, álbumes, artistas, listas y pistas
  public class Artista
  {
    // Necesarios
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string SpotifyArtistaId { get; set; }
    public string UrlImagen { get; set; }

    // Navegadores
    public List<Pista> Pistas { get; set; } = new();
    public List<Album> Albumes { get; set; } = new();
    public List<UsuarioSigueArtista> UsuariosQueSiguen { get; set; } = new();
  }

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

  public class HistorialLike
  {
    // Necesarios
    public int Id { get; set; }
    public TipoObjetoLike TipoObjetoLike { get; set; }
    public AccionLike AccionLike { get; set; }
    public DateTime Fecha { get; set; }

    // FKs
    public string UsuarioId { get; set; }
    public int ObjetoId { get; set; }

    // Navegadores
    public ApplicationUser Usuario { get; set; }
  }

  // Muchos a muchos: ListaReproduccion <-> Pista
  public class ListaPista
  {
    // Necesarios
    public int Id { get; set; }
    public int Posicion { get; set; }
    public DateTime AgregadoEn { get; set; }

    // FKs
    public int ListaId { get; set; }
    public int PistaId { get; set; }

    // Navegadores
    public ListaReproduccion Lista { get; set; }
    public Pista Pista { get; set; }
  }

  public class ListaReproduccion
  {
    // Necesarios
    public int Id { get; set; }
    public string Nombre { get; set; }
    public bool Publica { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateTime ActualizadoEn { get; set; }
    public string Descripcion { get; set; }
    public string SpotifyListaId { get; set; }
    public bool Sincronizada { get; set; }
    public bool Colaborativa { get; set; }

    // FKs
    public string UsuarioId { get; set; }

    // Navegadores
    public ApplicationUser Usuario { get; set; }
    public List<ListaPista> ListaPistas { get; set; } = new();
    public List<UsuarioLikeLista> UsuarioLikeListas { get; set; } = new();
    public List<UsuarioSigueLista> UsuarioSigueListas { get; set; } = new();
  }

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

  // NOTE: PerfilUsuario class has been removed - profile functionality is now integrated into ApplicationUser

  public class Pista
  {
    // Necesarios
    public int Id { get; set; }
    public string Titulo { get; set; }
    public int Duracion { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateTime ActualizadoEn { get; set; }
    public string Artista { get; set; }
    public string Album { get; set; }
    public string UrlPortada { get; set; }
    public DateTime? FechaLanzamiento { get; set; }
    public string SpotifyPistaId { get; set; }

    // FKs
    public int ArtistaId { get; set; }

    // Navegadores
    public Artista ArtistaNav { get; set; }
    public List<ListaPista> ListaPistas { get; set; } = new();
    public List<HistorialEscucha> HistorialEscuchas { get; set; } = new();
    public List<UsuarioLikePista> UsuarioLikePistas { get; set; } = new();
    public List<ArchivoSubido> ArchivosMusica { get; set; } = new();
  }

  // Suscripciones y planes
  public class PlanSuscripcion
  {
    // Necesarios
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int DuracionMeses { get; set; }
    public int MaxCuentas { get; set; }
    public bool RequiereVerificacionEstudiante { get; set; }
    public bool PermiteControlExplicito { get; set; }
    public DateTime FechaCreacion { get; set; }

    // Navegadores
    public List<Suscripcion> Suscripciones { get; set; } = new();
  }

  // 4. Solicitud de ascenso a Músico
  public class SolicitudMusico
  {
    // Necesarios
    public int Id { get; set; }
    public string UsuarioId { get; set; }
    public string? Mensaje { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public EstadoSolicitudMusico Estado { get; set; }
    public DateTime? FechaRevision { get; set; }
    public string? AdminRevisorId { get; set; }
    public string? MotivoRechazo { get; set; }

    // Navegadores
    public ApplicationUser Usuario { get; set; }
    public ApplicationUser? AdminRevisor { get; set; }
  }

  public class Suscripcion
  {
    // Necesarios
    public int Id { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public EstadoSuscripcion Estado { get; set; }

    // FKs
    public string UsuarioId { get; set; } // Titular de la suscripción
    public int PlanId { get; set; }

    // Navegadores
    public ApplicationUser Usuario { get; set; }
    public PlanSuscripcion Plan { get; set; }
    public List<SuscripcionUsuario> SuscripcionUsuarios { get; set; } = new();
    public List<TransaccionPago> TransaccionesPago { get; set; } = new();
  }

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

  // 1. Pagos/Transacciones
  public class TransaccionPago
  {
    // Necesarios
    public int Id { get; set; }
    public string UsuarioId { get; set; }
    public int? SuscripcionId { get; set; } // Opcional: pago de suscripción
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
    public EstadoPago Estado { get; set; }
    public ServicioPago Servicio { get; set; } // Simulado, Peigo, etc.
    public string? ReferenciaExterna { get; set; }
    public string? Detalle { get; set; }
    public string? JsonRespuesta { get; set; }

    // Navegadores
    public ApplicationUser Usuario { get; set; }
    public Suscripcion? Suscripcion { get; set; }
  }

  // ================================
  // RELATIONSHIP MODELS (Muchos a muchos)
  // ================================

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

  // Muchos a muchos: Usuario <-> ListaReproduccion (likes)
  public class UsuarioLikeLista
  {
    // Necesarios
    public int Id { get; set; }
    public DateTime CreadoEn { get; set; }

    // FKs
    public string UsuarioId { get; set; }
    public int ListaId { get; set; }

    // Navegadores
    public ApplicationUser Usuario { get; set; }
    public ListaReproduccion Lista { get; set; }
  }

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

  // Relaciones muchos a muchos de seguidores (usuario <-> usuario)
  public class UsuarioSigue
  {
    // Necesarios
    public int Id { get; set; }
    public DateTime CreadoEn { get; set; }

    // FKs
    public string SeguidorId { get; set; }
    public string SeguidoId { get; set; }

    // Navegadores
    public ApplicationUser Seguidor { get; set; }
    public ApplicationUser Seguido { get; set; }
  }

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

  // Muchos a muchos: Usuario <-> ListaReproduccion (seguimiento)
  public class UsuarioSigueLista
  {
    // Necesarios
    public int Id { get; set; }
    public DateTime CreadoEn { get; set; }

    // FKs
    public string UsuarioId { get; set; }
    public int ListaId { get; set; }

    // Navegadores
    public ApplicationUser Usuario { get; set; }
    public ListaReproduccion Lista { get; set; }
  }
}

// ================================
// ADDITIONAL MODELS (Different Namespace)
// ================================

namespace Melodix.MVC.Models
{
  public class ErrorViewModel
  {
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
  }
}
