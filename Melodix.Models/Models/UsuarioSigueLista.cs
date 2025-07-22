using Melodix.Models.Models;

namespace Melodix.Models
{
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