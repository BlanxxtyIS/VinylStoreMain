using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VinylShop.Shared.Models
{
    public class AlbumDto
    {
        public int AlbumId { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Image { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public int TimeInMinutes { get; set; }
        public string TimeFormatted => $"{TimeInMinutes / 60}h {TimeInMinutes % 60}m";
        public int Raiting { get; set; }
        public IEnumerable<Track> Tracks { get; set; } = Array.Empty<Track>();

        public class Track
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }
    }
}
