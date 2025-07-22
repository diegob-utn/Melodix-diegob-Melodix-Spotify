namespace Melodix.Models
{
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
}