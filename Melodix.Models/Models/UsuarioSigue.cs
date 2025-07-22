using Melodix.Models.Models;

namespace Melodix.Models
{
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
}