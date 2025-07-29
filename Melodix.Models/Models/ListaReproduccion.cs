using Melodix.Models.Models;

namespace Melodix.Models
{
    public class ListaReproduccion
    {
        // Necesarios
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Publica { get; set; }
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string SpotifyListaId { get; set; } = string.Empty;
        public bool Sincronizada { get; set; }
        public bool Colaborativa { get; set; }

        // FKs
        public string UsuarioId { get; set; } = string.Empty;

        // Navegadores
        public ApplicationUser Usuario { get; set; } = null!;
        public List<ListaPista> ListasPista { get; set; } = new();
        public List<UsuarioLikeLista> UsuarioLikeListas { get; set; } = new();
        public List<UsuarioSigueLista> UsuarioSigueListas { get; set; } = new();
    }
}