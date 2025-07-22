using System.Collections.Generic;

namespace Melodix.Models.Models
{
    public class SpotifyUserProfile
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public List<Image> Images { get; set; } = new();
        public string Country { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
    }

    public class Image
    {
        public string Url { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
