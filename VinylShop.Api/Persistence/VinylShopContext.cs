using Microsoft.EntityFrameworkCore;
using VinylShop.Shared.Models;

namespace VinylShop.Api.Persistence
{
    public class VinylShopContext : DbContext
    {
        public DbSet<Album> Albums => Set<Album>();
        public DbSet<Track> Tracks => Set<Track>();

        public VinylShopContext(DbContextOptions options) : base(options)
        {
        }
    }
}
