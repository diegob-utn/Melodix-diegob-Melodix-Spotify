namespace Melodix.Models
{
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
}