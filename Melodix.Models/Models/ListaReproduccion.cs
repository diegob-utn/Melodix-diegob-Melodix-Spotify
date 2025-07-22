using Melodix.Models.Models;

namespace Melodix.Models
{
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
}