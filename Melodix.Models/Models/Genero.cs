using System.ComponentModel.DataAnnotations;

namespace Melodix.Models
{
  public class Genero
  {
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    public virtual List<Pista> Pistas { get; set; } = new();
  }
}
