using Melodix.Models;
using Melodix.Models.Models;

namespace Melodix.Models
{
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
}