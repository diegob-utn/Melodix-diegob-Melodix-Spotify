using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Melodix.Models;
using Melodix.Models.Models;

namespace Melodix.MVC.ViewModels
{
  /// <summary>
  /// ViewModel para el formulario de login
  /// </summary>
  public class LoginViewModel
  {
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "Ingresa un email válido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Recordarme")]
    public bool RememberMe { get; set; }
  }

  /// <summary>
  /// ViewModel para el formulario de registro
  /// </summary>
  public class RegisterViewModel
  {
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nick es requerido")]
    [StringLength(50, ErrorMessage = "El nick no puede exceder 50 caracteres")]
    public string Nick { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "Ingresa un email válido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma tu contraseña")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmarPassword { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime? FechaNacimiento { get; set; }

    public GeneroUsuario? Genero { get; set; }

    [Required(ErrorMessage = "Debes aceptar los términos y condiciones")]
    public bool AceptaTerminos { get; set; }
  }

  /// <summary>
  /// ViewModel para mostrar perfil de usuario
  /// </summary>
  public class PerfilViewModel
  {
    public ApplicationUser Usuario { get; set; } = null!;
    public bool EsPropietario { get; set; }
    public bool YaSigue { get; set; }
    public int TotalEscuchas { get; set; }
    public int TotalListas { get; set; }
    public int TotalSeguidores { get; set; }
    public int TotalSiguiendo { get; set; }
    public List<ListaReproduccion> ListasPublicas { get; set; } = new();
    public List<Pista> PistasRecientes { get; set; } = new();
  }

  /// <summary>
  /// ViewModel para editar perfil
  /// </summary>
  public class EditPerfilViewModel
  {
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nick es requerido")]
    [StringLength(50, ErrorMessage = "El nick no puede exceder 50 caracteres")]
    public string Nick { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La biografía no puede exceder 255 caracteres")]
    public string? Biografia { get; set; }

    [StringLength(100, ErrorMessage = "La ubicación no puede exceder 100 caracteres")]
    public string? Ubicacion { get; set; }

    [DataType(DataType.Date)]
    public DateTime? FechaNacimiento { get; set; }

    public GeneroUsuario? Genero { get; set; }

    public string? FotoPerfil { get; set; }

    [Display(Name = "Foto de perfil")]
    public IFormFile? ArchivoFoto { get; set; }
  }

  /// <summary>
  /// ViewModel para mostrar el feed principal
  /// </summary>
  public class InicioViewModel
  {
    public List<Album> AlbumsDestacados { get; set; } = new();
    public List<ListaReproduccion> ListasRecomendadas { get; set; } = new();
    public List<Pista> PistasRecientes { get; set; } = new();
    public List<Pista> PistasTendencia { get; set; } = new();
    public ApplicationUser? UsuarioActual { get; set; }
  }

  /// <summary>
  /// ViewModel para búsqueda
  /// </summary>
  public class BusquedaViewModel
  {
    public string Query { get; set; } = string.Empty;
    public string? Tipo { get; set; }
    public List<Pista> Pistas { get; set; } = new();
    public List<Album> Albums { get; set; } = new();
    public List<ListaReproduccion> Listas { get; set; } = new();
    public List<ListaReproduccion> Playlists { get; set; } = new();
    public List<ApplicationUser> Usuarios { get; set; } = new();
    public List<ApplicationUser> Artistas { get; set; } = new();
    public int TotalResultados => Pistas.Count + Albums.Count + Listas.Count + Usuarios.Count;
  }

  /// <summary>
  /// ViewModel para biblioteca personal
  /// </summary>
  public class BibliotecaViewModel
  {
    public List<ListaReproduccion> MisListas { get; set; } = new();
    public List<Album> AlbumsGuardados { get; set; } = new();
    public List<Pista> PistasGuardadas { get; set; } = new();
    public List<ApplicationUser> ArtistasSeguidores { get; set; } = new();
    public ApplicationUser? UsuarioActual { get; set; }
  }

  /// <summary>
  /// ViewModel para playlist/lista de reproducción
  /// </summary>
  public class PlaylistViewModel
  {
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }

    public bool Publica { get; set; } = true;

    public bool Colaborativa { get; set; } = false;

    public List<Pista> Pistas { get; set; } = new();

    public ApplicationUser? Propietario { get; set; }

    public bool EsPropietario { get; set; }

    public int TotalDuracion { get; set; }

    public DateTime? CreadoEn { get; set; }
  }

  /// <summary>
  /// ViewModel para el reproductor
  /// </summary>
  public class ReproductorViewModel
  {
    public Pista? PistaActual { get; set; }
    public List<Pista> Cola { get; set; } = new();
    public int IndiceActual { get; set; }
    public bool EsAleatorio { get; set; }
    public bool EsRepetir { get; set; }
    public double Volumen { get; set; } = 0.5;
  }

  /// <summary>
  /// ViewModel para subir pistas (músicos)
  /// </summary>
  public class PistaViewModel
  {
    [Required(ErrorMessage = "El título es requerido")]
    [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona un archivo de audio")]
    public IFormFile ArchivoAudio { get; set; } = null!;

    public IFormFile? ArchivoPortada { get; set; }

    public int? AlbumId { get; set; }

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }

    public DateTime? FechaLanzamiento { get; set; }

    public List<Album> AlbumsDisponibles { get; set; } = new();
  }

  /// <summary>
  /// ViewModel para panel de administración
  /// </summary>
  public class AdminPanelViewModel
  {
    public int TotalUsuarios { get; set; }
    public int TotalPistas { get; set; }
    public int TotalAlbums { get; set; }
    public int TotalListas { get; set; }
    public List<SolicitudMusico> SolicitudesPendientes { get; set; } = new();
    public List<LogSistema> LogsRecientes { get; set; } = new();
    public List<ApplicationUser> UsuariosRecientes { get; set; } = new();
  }

  // === SUSCRIPCION VIEWMODELS ===
  public class SuscripcionViewModel
  {
    public List<PlanSuscripcion> Planes { get; set; } = new();
    public Suscripcion? SuscripcionActual { get; set; }
    public ApplicationUser Usuario { get; set; } = null!;
    public bool TieneSuscripcionActiva { get; set; }
  }

  // === ARTISTA VIEWMODELS ===
  public class ArtistaDashboardViewModel
  {
    public ApplicationUser Artista { get; set; } = null!;
    public int TotalPistas { get; set; }
    public int TotalAlbumes { get; set; }
    public int TotalSeguidores { get; set; }
    public int TotalReproduccionesMes { get; set; }
    public int TotalReproduccionesTotales { get; set; }
    public int TotalLikes { get; set; }
    public List<Pista> UltimasPistas { get; set; } = new();
    public List<Pista> PistasPopulares { get; set; } = new();
    public List<Pista> PistasRecientes { get; set; } = new();
    public List<Pista> PistasMasPopulares { get; set; } = new();
  }

  public class MisPistasViewModel
  {
    public List<Pista> Pistas { get; set; } = new();
    public int PaginaActual { get; set; }
    public int TotalPaginas { get; set; }
    public string? TerminoBusqueda { get; set; }
    public int TotalResultados { get; set; }
  }

  public class SubirPistaViewModel
  {
    [Required(ErrorMessage = "El título es obligatorio")]
    [StringLength(100, ErrorMessage = "El título no puede exceder 100 caracteres")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe seleccionar un archivo de audio")]
    [Display(Name = "Archivo de audio")]
    public IFormFile ArchivoAudio { get; set; } = null!;

    [Display(Name = "Álbum")]
    public int? AlbumId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un género")]
    [Display(Name = "Género")]
    public GeneroMusica Genero { get; set; } = GeneroMusica.Desconocido;

    [Display(Name = "Contenido explícito")]
    public bool EsExplicita { get; set; }
  }

  public class EditarPistaViewModel
  {
    public int Id { get; set; }

    [Required(ErrorMessage = "El título es obligatorio")]
    [StringLength(100, ErrorMessage = "El título no puede exceder 100 caracteres")]
    public string Titulo { get; set; } = string.Empty;

    [Display(Name = "Álbum")]
    public int? AlbumId { get; set; }

    [Display(Name = "Género")]
    public GeneroMusica Genero { get; set; } = GeneroMusica.Desconocido;

    [Display(Name = "Contenido explícito")]
    public bool EsExplicita { get; set; }
  }

  public class EstadisticasArtistaViewModel
  {
    public int TotalPistas { get; set; }
    public int TotalAlbumes { get; set; }
    public int TotalSeguidores { get; set; }
    public int TotalReproduccionesTotales { get; set; }
    public List<Pista> PistasMasPopulares { get; set; } = new();
    public List<ReproduccionesMesViewModel> ReproduccionesPorMes { get; set; } = new();
  }

  public class ReproduccionesMesViewModel
  {
    public int Año { get; set; }
    public int Mes { get; set; }
    public int Cantidad { get; set; }
  }

  public class CrearAlbumViewModel
  {
    [Required(ErrorMessage = "El título es obligatorio")]
    [StringLength(100, ErrorMessage = "El título no puede exceder 100 caracteres")]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "La fecha de lanzamiento es obligatoria")]
    [Display(Name = "Fecha de lanzamiento")]
    public DateTime FechaLanzamiento { get; set; }
  }

  // === ADMIN VIEWMODELS ===
  public class AdminDashboardViewModel
  {
    public int TotalUsuarios { get; set; }
    public int TotalArtistas { get; set; }
    public int TotalPistas { get; set; }
    public int TotalAlbumes { get; set; }
    public int TotalListas { get; set; }
    public int TotalGeneros { get; set; }
    public int UsuariosActivos { get; set; }
    public int UsuariosRecientes { get; set; }
    public int PistasRecientes { get; set; }
    public int ReproduccionesHoy { get; set; }
    public int SuscripcionesActivas { get; set; }
    public int SolicitudesPendientes { get; set; }
    public List<UsuarioResumeViewModel> UltimosUsuarios { get; set; } = new();
    public List<Pista> PistasMasPopulares { get; set; } = new();
  }

  public class GestionUsuariosViewModel
  {
    public List<UsuarioConRolViewModel> Usuarios { get; set; } = new();
    public int PaginaActual { get; set; }
    public int TotalPaginas { get; set; }
    public string? TerminoBusqueda { get; set; }
    public string? RolFiltro { get; set; }
    public int TotalResultados { get; set; }
  }

  public class UsuarioConRolViewModel
  {
    public ApplicationUser Usuario { get; set; } = null!;
    public List<string> Roles { get; set; } = new();
  }

  public class GestionMusicaViewModel
  {
    public List<Pista> Pistas { get; set; } = new();
    public int PaginaActual { get; set; }
    public int TotalPaginas { get; set; }
    public string? TerminoBusqueda { get; set; }
    public int TotalResultados { get; set; }
  }

  public class LogsSistemaViewModel
  {
    public List<LogSistema> Logs { get; set; } = new();
    public int PaginaActual { get; set; }
    public int TotalPaginas { get; set; }
    public string? FiltroAccion { get; set; }
    public int TotalResultados { get; set; }
  }

  public class ReportesViewModel
  {
    public List<RegistrosMesViewModel> RegistrosPorMes { get; set; } = new();
    public List<Pista> PistasMasPopulares { get; set; } = new();
    public List<GeneroPopularViewModel> GenerosMasPopulares { get; set; } = new();
  }

  public class RegistrosMesViewModel
  {
    public int Año { get; set; }
    public int Mes { get; set; }
    public int Cantidad { get; set; }
  }

  public class GeneroPopularViewModel
  {
    public string Nombre { get; set; } = string.Empty;
    public int CantidadPistas { get; set; }
    public int TotalReproducciones { get; set; }
  }

  /// <summary>
  /// ViewModels adicionales para AdminController
  /// </summary>
  public class UsuarioResumeViewModel
  {
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Nick { get; set; } = string.Empty;
    public DateTime? FechaRegistro { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    public bool EstaBloqueado { get; set; }
  }

  public class AdminUsuariosViewModel
  {
    public List<UsuarioResumeViewModel> Usuarios { get; set; } = new();
    public int PaginaActual { get; set; }
    public int TotalPaginas { get; set; }
    public string? Busqueda { get; set; }
    public int TotalUsuarios { get; set; }
  }

  public class AdminDetalleUsuarioViewModel
  {
    public ApplicationUser Usuario { get; set; } = null!;
    public List<string> Roles { get; set; } = new();
    public int TotalPistas { get; set; }
    public int TotalAlbumes { get; set; }
    public int TotalListas { get; set; }
    public int TotalReproducciones { get; set; }
    public Suscripcion? SuscripcionActual { get; set; }
  }

  public class AdminContenidoViewModel
  {
    public List<Album> Albums { get; set; } = new();
    public List<Pista> Pistas { get; set; } = new();
    public int PaginaActual { get; set; }
    public int TotalPaginas { get; set; }
    public string? Busqueda { get; set; }
    public string? TipoFiltro { get; set; }
  }

  public class GeneroViewModel
  {
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int TotalPistas { get; set; }
  }

  public class CrearGeneroViewModel
  {
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres")]
    public string? Descripcion { get; set; }
  }

  public class AdminReportesViewModel
  {
    public List<EstadisticaMensualViewModel> UsuariosPorMes { get; set; } = new();
    public List<EstadisticaMensualViewModel> ReproduccionesPorMes { get; set; } = new();
    public List<GeneroPopularidadViewModel> GenerosMasPopulares { get; set; } = new();
  }

  public class EstadisticaMensualViewModel
  {
    public int Año { get; set; }
    public int Mes { get; set; }
    public int Cantidad { get; set; }
  }

  public class GeneroPopularidadViewModel
  {
    public string Nombre { get; set; } = string.Empty;
    public int TotalPistas { get; set; }
    public int TotalReproducciones { get; set; }
  }

  public class PerfilUsuarioViewModel
  {
    public ApplicationUser? Usuario { get; set; }
    public bool EsPropioUsuario { get; set; }
    public int TotalSeguidores { get; set; }
    public int TotalSiguiendo { get; set; }
    public int TotalPistas { get; set; }
    public int TotalPlaylists { get; set; }
    public long TotalReproducciones { get; set; }
    public int TotalLikes { get; set; }
    public List<Pista>? PistasPublicas { get; set; }
    public List<ListaReproduccion>? PlaylistsPublicas { get; set; }
    public List<string>? ActividadReciente { get; set; }
    public List<string>? GenerosPreferidos { get; set; }
    public bool EsArtista { get; set; }
  }

  /// <summary>
  /// ViewModel para los planes de suscripción
  /// </summary>
  public class SuscripcionPlanesViewModel
  {
    public List<PlanSuscripcion> Planes { get; set; } = new();
    public PlanSuscripcion? PlanActual { get; set; }
    public SuscripcionUsuario? SuscripcionActual { get; set; }
  }
}
