using Melodix.Models.Models;
using Melodix.Models;

namespace Melodix.MVC.ViewModels
{
  public class MusicPlayerViewModel
  {
    public List<PistaMusicPlayerViewModel> Pistas { get; set; } = new List<PistaMusicPlayerViewModel>();
    public bool PuedeSubir { get; set; } = true; // Por defecto permitir subir
    public List<GeneroMusica> Generos { get; set; } = new List<GeneroMusica>();
  }

  public class PistaMusicPlayerViewModel
  {
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Artista { get; set; } = string.Empty;
    public string Album { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public GeneroMusica Genero { get; set; }
    public bool EsExplicita { get; set; }
    public int Reproducciones { get; set; }
    public DateTime FechaSubida { get; set; }
    public bool EsMia { get; set; } // Para saber si el usuario actual puede eliminarla
  }
}
